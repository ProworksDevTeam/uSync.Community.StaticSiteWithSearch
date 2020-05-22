using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using uSync.Publisher;
using uSync.Publisher.Configuration;
using uSync.Publisher.Models;
using uSync.Publisher.Publishers;
using uSync.Publisher.Services;
using uSync.Publisher.Static;
using uSync8.BackOffice;
using uSync8.BackOffice.Services;
using uSync8.Core.Dependency;


namespace uSync.Community.StaticSiteWithSearch.Publisher
{
    public class ExtensibleStaticPublisher : SyncStepImportBase, IStepPublisher
    {
        public const string PublisherAlias = "static-ext";

        private readonly uSyncOutgoingService _outgoingService;
        private readonly uSyncStaticSiteService _staticSiteService;
        private readonly IContentService _contentService;
        private readonly SyncFileService _syncFileService;
        private readonly Dictionary<IStaticSitePublisherExtension, object> _staticSitePublisherExtensions;
        private readonly string _syncRoot;

        public string Name => "Extensible Static Site Publisher";
        public string Alias => PublisherAlias;

        public ExtensibleStaticPublisher(
          uSyncPublisherConfig config,
          IProfilingLogger logger,
          IGlobalSettings settings,
          uSyncOutgoingService outgoingService,
          uSyncIncomingService incomingService,
          uSyncStaticSiteService staticSiteService,
          IContentService contentService,
          SyncFileService syncFileService,
          IEnumerable<IStaticSitePublisherExtension> staticSitePublisherExtensions)
          : base(config, logger, settings, incomingService)
        {
            _outgoingService = outgoingService;
            _staticSiteService = staticSiteService;
            _contentService = contentService;
            _syncFileService = syncFileService;
            _staticSitePublisherExtensions = (staticSitePublisherExtensions?.ToList() ?? new List<IStaticSitePublisherExtension>()).ToDictionary(e => e, e => (object)null);
            _syncRoot = Path.Combine(settings.LocalTempPath, "uSync", "pack");
            Actions = new Dictionary<PublishMode, IEnumerable<SyncPublisherAction>>()
            {
                {
                    PublishMode.Push,
                    PushActions
                }
            };
        }

        public override Task<SyncServerStatus> GetStatus(string target)
        {
            return Task.FromResult(SyncServerStatus.Success);
        }

        public IEnumerable<SyncPublisherAction> PushActions
        {
            get
            {
                return new List<SyncPublisherAction>
                {
                    new SyncPublisherAction("config", "Publish", new SyncPublisherStep("Publish Site", "icon-document"), new Func<Guid, SyncPublisherAction, ActionArguments, Task<StepActionResult>>(Start))
                    {
                        View = uSyncPublisher.PluginFolder + "publishers/static/config.html",
                        ButtonText = "Publish Site"
                    },
                    new SyncPublisherAction("Publish", "Publish Files", new Func<Guid, SyncPublisherAction, ActionArguments, Task<StepActionResult>>(Publish))
                    {
                        Steps = new List<SyncPublisherStep>()
                        {
                            new SyncPublisherStep("Calculate", "icon-science", "Calculating"),
                            new SyncPublisherStep("Creating Pages", "icon-box", "Files"),
                            new SyncPublisherStep("Gathering Media", "icon-picture", "Media"),
                            new SyncPublisherStep("Files", "icon-documents", "Files"),
                            new SyncPublisherStep("Upload", "icon-truck usync-truck", "Uploading")
                        }
                    },
                    new SyncPublisherAction("result", "Publish Results", new SyncPublisherStep("Publish", "icon-result"), new Func<Guid, SyncPublisherAction, ActionArguments, Task<StepActionResult>>(Complete))
                    {
                        View = uSyncPublisher.PluginFolder + "publishers/static/result.html"
                    }
                };
            }
        }

        private void RunExtension(Action<IStaticSitePublisherExtension, object> method)
        {
            foreach (var pair in _staticSitePublisherExtensions)
            {
                try
                {
                    method(pair.Key, pair.Value);
                }
                catch (Exception ex)
                {
                    logger.Error<ExtensibleStaticPublisher>($"Could not run the {pair.Key?.GetType().FullName} extension", ex);
                }
            }
        }

        public async Task<StepActionResult> Publish(
          Guid id,
          SyncPublisherAction action,
          ActionArguments args)
        {
            if (args?.Options == null)
                throw new ArgumentNullException(nameof(args));
            if (id == Guid.Empty)
                id = Guid.NewGuid();

            _staticSitePublisherExtensions.Keys.ToList().ForEach(e => _staticSitePublisherExtensions[e] = e.BeginPublish(id, _syncRoot, action, args));
            var itemDependencies = _outgoingService.GetItemDependencies(args.Options.Items, args.Callbacks)?.ToList() ?? new List<uSyncDependency>();
            RunExtension((e, s) => e.AddCustomDependencies(s, itemDependencies));

            MoveToNextStep(action, args);
            GenerateHtml(itemDependencies, id, args);
            MoveToNextStep(action, args);
            GatherMedia(itemDependencies, id, args);
            MoveToNextStep(action, args);
            GatherFiles(id, args);
            RunExtension((e, s) => e.BeforeFinalPublish(s));
            MoveToNextStep(action, args);
            Publish(id, args);
            RunExtension((e, s) => e.EndPublish(s));

            return await Task.FromResult(new StepActionResult(true, id, args.Options, Enumerable.Empty<uSyncAction>()));
        }

        private void GenerateHtml(IEnumerable<uSyncDependency> dependencies, Guid id, ActionArguments args)
        {
            var list = dependencies.Where(x => x.Udi.EntityType == "document").ToList();
            if (list == null || !list.Any()) return;

            var count = list.Count;

            foreach (var (page, index) in list.Select((page, index) => (page, index)))
            {
                int itemId = _staticSiteService.GetItemId(page.Udi);
                if (itemId > 0)
                {
                    args.Callbacks?.Update?.Invoke("Generating: " + page.Name + " html", index, count);
                    SaveHtml(id, itemId);
                }
            }
        }

        private bool SaveHtml(Guid packId, int itemId)
        {
            try
            {
                var byId = _contentService.GetById(itemId);
                if (byId != null && byId.Published)
                {
                    var text = _staticSiteService.GenerateItemHtml(itemId);
                    var itemPath = _staticSiteService.GetItemPath(itemId);

                    RunExtension((e, s) => text = e.TransformHtml(s, byId, itemPath, text));
                    if (string.IsNullOrWhiteSpace(text)) return false;

                    var path = $"{_syncRoot}/{packId}/{itemPath}/index.html".Replace("/", "\\");
                    _syncFileService.SaveFile(path, text);
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Warn<uSyncStaticSiteService>("Error Saving Html", ex);
            }
            return false;
        }

        private void GatherMedia(IEnumerable<uSyncDependency> dependencies, Guid id, ActionArguments args)
        {
            var list = dependencies.Where(x => x.Udi.EntityType == "media").ToList();
            foreach (var (media, index) in list.Select((media, index) => (media, index)))
            {
                uSyncCallbacks callbacks = args.Callbacks;
                if (callbacks != null)
                {
                    callbacks.Update?.Invoke("Saving: " + media.Name, index, list.Count);
                }

                _staticSiteService.SaveMedia(id, media.Udi);
            }
        }

        private void GatherFiles(Guid id, ActionArguments args)
        {
            if (!args.Options.IncludeFileHash)
                return;

            var folders = new List<string> { "~/css", "~/scripts" };
            RunExtension((e, s) => e.AddCustomFolders(s, folders));
            _staticSiteService.SaveFolders(id, folders.ToArray());
        }

        private void Publish(Guid id, ActionArguments args)
        {
            _staticSiteService.Publish(id, args.Target, args.Callbacks?.Update);
        }
    }
}
