using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;
using Umbraco.Core.Services;
using Umbraco.Web;
using uSync.Community.StaticSiteWithSearch.Config;
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
        private readonly IPublisherSearchConfigs _publisherSearchConfigs;
        private readonly UmbracoHelper _umbracoHelper;
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
          IEnumerable<IStaticSitePublisherExtension> staticSitePublisherExtensions,
          IPublisherSearchConfigs publisherSearchConfigs,
          UmbracoHelper umbracoHelper)
          : base(config, logger, settings, incomingService)
        {
            _outgoingService = outgoingService;
            _staticSiteService = staticSiteService;
            _contentService = contentService;
            _syncFileService = syncFileService;
            _publisherSearchConfigs = publisherSearchConfigs;
            _umbracoHelper = umbracoHelper;
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

        public override async Task<SyncServerStatus> GetStatus(string target)
        {
            var config = _publisherSearchConfigs.ConfigsByServerName.TryGetValue(target, out var sc) ? sc : null;
            if (config?.Deployer == null) return SyncServerStatus.Success;

            return await config.Deployer.CheckStatus(config.DeployerConfig);
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

        public Task<StepActionResult> Publish(
          Guid id,
          SyncPublisherAction action,
          ActionArguments args)
        {
            if (args?.Options == null)
                throw new ArgumentNullException(nameof(args));

            try
            {
                if (id == Guid.Empty)
                    id = Guid.NewGuid();

                var config = _publisherSearchConfigs.ConfigsByServerName.TryGetValue(args.Target, out var sc) ? sc : null;
                _staticSitePublisherExtensions.Keys.ToList().ForEach(e => _staticSitePublisherExtensions[e] = e.BeginPublish(id, _syncRoot, action, args, config));
                var itemDependencies = _outgoingService.GetItemDependencies(args.Options.Items, args.Callbacks)?.ToList() ?? new List<uSyncDependency>();
                var itemPaths = new Dictionary<int, string>(itemDependencies.Count) { [-1] = "/" };
                RunExtension((e, s) => e.AddCustomDependencies(s, itemDependencies));

                MoveToNextStep(action, args);
                GenerateHtml(itemDependencies, id, args, itemPaths);
                MoveToNextStep(action, args);
                GatherMedia(itemDependencies, id, args);
                MoveToNextStep(action, args);
                GatherFiles(id, args);
                RunExtension((e, s) => e.BeforeFinalPublish(s));
                MoveToNextStep(action, args);
                Publish(id, args, config, itemPaths);
                RunExtension((e, s) => e.EndPublish(s));

                var result = new StepActionResult(true, id, args.Options, Enumerable.Empty<uSyncAction>());
                return Task.FromResult(result);
            }
            catch (Exception ex)
            {
                logger.Error<ExtensibleStaticPublisher>($"Could not publish", ex);
                throw;
            }
        }

        private void GenerateHtml(IEnumerable<uSyncDependency> dependencies, Guid id, ActionArguments args, Dictionary<int, string> itemPaths)
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
                    SaveHtml(id, itemId, itemPaths);
                }
            }
        }

        private bool SaveHtml(Guid packId, int itemId, Dictionary<int, string> itemPaths)
        {
            try
            {
                var byId = _contentService.GetById(itemId);
                if (byId != null && byId.Published)
                {
                    var text = _staticSiteService.GenerateItemHtml(itemId);
                    if (text != null && text.StartsWith("<!-- Error rendering template") && text.EndsWith(" -->"))
                    {
                        logger.Warn<ExtensibleStaticPublisher>(text.Substring(5, text.Length - 9));
                        return false;
                    }
                    
                    var itemPath = _staticSiteService.GetItemPath(itemId);
                    itemPaths[itemId] = itemPath;

                    RunExtension((e, s) => text = e.TransformHtml(s, byId, itemPath, text));
                    if (string.IsNullOrWhiteSpace(text)) return false;

                    var path = $"{_syncRoot}/{packId}/{itemPath}/index.html".Replace("/", "\\");
                    _syncFileService.SaveFile(path, text);
                    return true;
                }
            }
            catch (Exception ex)
            {
                logger.Warn<ExtensibleStaticPublisher>("Error Saving Html", ex);
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
            var files = new Dictionary<string, Stream>();

            RunExtension((e, s) => e.AddCustomFilesAndFolders(s, folders, files));

            if (folders.Count > 0) _staticSiteService.SaveFolders(id, folders.ToArray());
            if (files.Count > 0)
            {
                foreach (var file in files)
                {
                    try
                    {
                        var path = $"{_syncRoot}/{id}/{file.Key}".Replace("/", "\\");
                        _syncFileService.SaveFile(path, file.Value);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn<ExtensibleStaticPublisher>($"Error saving file {file.Key}", ex);
                    }
                    finally
                    {
                        file.Value?.Dispose();
                    }
                }
            }
        }

        private void Publish(Guid id, ActionArguments args, IPublisherSearchConfig config, Dictionary<int, string> itemPaths)
        {
            var folder = $"{_syncRoot}/{id}";
            if (config.Deployer != null && args.Options.DeleteMissing)
            {
                // Find all items being published that are publishing their children as well
                var roots = args.Options.Items.Select(i => i.flags.HasFlag(DependencyFlags.IncludeChildren) && itemPaths.TryGetValue(i.Id, out var path) ? path : null).Where(i => i != null).ToList();

                // Remove any that are sub-folders of another one in the list
                roots.RemoveAll(r => roots.Any(rt => r != rt && r.StartsWith(rt)));

                if (roots.Count > 0) config.Deployer.RemovePathsIfExist(config.DeployerConfig, roots);
            }
            config.LimitedDeployer.Deploy(folder, config.DeployerConfig, args.Callbacks?.Update);
        }
    }
}
