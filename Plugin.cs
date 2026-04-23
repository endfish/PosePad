using Dalamud.Game.Command;
using Dalamud.IoC;
using Dalamud.Interface.Windowing;
using Dalamud.Plugin;
using Dalamud.Plugin.Services;
using PosePad.Configuration;
using PosePad.Integrations.Penumbra;
using PosePad.Localization;
using PosePad.Services;
using PosePad.Windows;

namespace PosePad;

public sealed class Plugin : IDalamudPlugin
{
    [PluginService] internal static IDalamudPluginInterface PluginInterface { get; private set; } = null!;
    [PluginService] internal static ICommandManager CommandManager { get; private set; } = null!;
    [PluginService] internal static IDataManager DataManager { get; private set; } = null!;
    [PluginService] internal static IClientState ClientState { get; private set; } = null!;
    [PluginService] internal static IFramework Framework { get; private set; } = null!;
    [PluginService] internal static IPluginLog Log { get; private set; } = null!;
    [PluginService] internal static IObjectTable ObjectTable { get; private set; } = null!;
    [PluginService] internal static ITargetManager TargetManager { get; private set; } = null!;
    [PluginService] internal static IToastGui ToastGui { get; private set; } = null!;

    private const string CommandName = "/posepad";
    private const string CommandAlias = "/posspad";

    public PluginConfiguration Configuration { get; }
    public WindowSystem WindowSystem { get; } = new("PosePad");

    public ActionCatalogService CatalogService { get; }
    public ActionExecutionService ExecutionService { get; }
    public ActorResolverService ActorResolverService { get; }
    public IPenumbraIntegration PenumbraIntegration { get; }

    private readonly ConfigWindow configWindow;
    private readonly MainWindow mainWindow;
    private bool lastIsGPosing;

    public Plugin()
    {
        Configuration = PluginInterface.GetPluginConfig() as PluginConfiguration ?? new PluginConfiguration();
        lastIsGPosing = ClientState.IsGPosing;

        var commonActionRepository = new CommonActionRepository(
            Path.Combine(PluginInterface.AssemblyLocation.Directory?.FullName ?? string.Empty, "Data", "CommonActions.json"),
            Log);
        var emoteActionRepository = new EmoteActionRepository(DataManager, Log);
        ActorResolverService = new ActorResolverService(ClientState, ObjectTable, TargetManager, Log);
        var timelinePlaybackService = new TimelinePlaybackService();
        PenumbraIntegration = CreatePenumbraIntegration(emoteActionRepository);

        CatalogService = new ActionCatalogService(commonActionRepository, emoteActionRepository, PenumbraIntegration, Configuration);
        ExecutionService = new ActionExecutionService(
            Configuration,
            CatalogService,
            ActorResolverService,
            timelinePlaybackService,
            new EmoteActionExecutor(Configuration, ActorResolverService, timelinePlaybackService, Log),
            new TimelineActionExecutor(Configuration, ActorResolverService, timelinePlaybackService, Log),
            ToastGui,
            Log);

        configWindow = new ConfigWindow(this);
        mainWindow = new MainWindow(this);

        WindowSystem.AddWindow(configWindow);
        WindowSystem.AddWindow(mainWindow);

        var commandInfo = new CommandInfo(OnCommand)
        {
            HelpMessage = "Open the PosePad main window. Aliases: /posepad, /posspad."
        };

        CommandManager.AddHandler(CommandName, commandInfo);
        CommandManager.AddHandler(CommandAlias, commandInfo);

        PluginInterface.UiBuilder.Draw += WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi += OpenConfigUi;
        PluginInterface.UiBuilder.OpenMainUi += OpenMainUi;
        PluginInterface.UiBuilder.DisableGposeUiHide = Configuration.DisableGposeUiHide;
        Framework.Update += OnFrameworkUpdate;

        Log.Information("PosePad initialized.");
    }

    public void Dispose()
    {
        PluginInterface.UiBuilder.Draw -= WindowSystem.Draw;
        PluginInterface.UiBuilder.OpenConfigUi -= OpenConfigUi;
        PluginInterface.UiBuilder.OpenMainUi -= OpenMainUi;
        PluginInterface.UiBuilder.DisableGposeUiHide = false;
        Framework.Update -= OnFrameworkUpdate;

        WindowSystem.RemoveAllWindows();
        configWindow.Dispose();
        mainWindow.Dispose();
        if (PenumbraIntegration is IDisposable disposableIntegration)
            disposableIntegration.Dispose();

        CommandManager.RemoveHandler(CommandName);
        CommandManager.RemoveHandler(CommandAlias);
    }

    public void OpenMainUi()
    {
        mainWindow.IsOpen = true;
        Configuration.MainWindow.IsOpen = true;
        Configuration.Save();
    }

    public void OpenConfigUi()
    {
        configWindow.IsOpen = true;
        Configuration.ConfigWindow.IsOpen = true;
        Configuration.Save();
    }

    public void ApplyUiVisibilitySettings()
    {
        PluginInterface.UiBuilder.DisableGposeUiHide = Configuration.DisableGposeUiHide;
    }

    private void OnCommand(string command, string args)
    {
        mainWindow.Toggle();
        Configuration.MainWindow.IsOpen = mainWindow.IsOpen;
        Configuration.Save();
    }

    private void OnFrameworkUpdate(IFramework framework)
    {
        var isGPosing = ClientState.IsGPosing;
        if (!lastIsGPosing && isGPosing && Configuration.OpenOnEnterGPose && !mainWindow.IsOpen)
            OpenMainUi();

        lastIsGPosing = isGPosing;
    }

    private IPenumbraIntegration CreatePenumbraIntegration(EmoteActionRepository emoteActionRepository)
    {
        try
        {
            return new PenumbraIpcIntegration(PluginInterface, Configuration, ObjectTable, Log, emoteActionRepository);
        }
        catch (Exception ex)
        {
            Log.Warning(ex, "Failed to initialize Penumbra integration. Falling back to null integration.");
            return new NullPenumbraIntegration();
        }
    }
}
