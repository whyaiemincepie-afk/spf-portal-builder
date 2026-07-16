namespace SpfPortalBuilder.Services;

public class DashboardService
{
    public List<Dashboard> Dashboards { get; } = new();

    public Guid CreateDashboard(string name, string? description = null)
    {
        var dashboard = new Dashboard
        {
            Id = Guid.NewGuid(),
            Name = name,
            Description = description ?? "",
            Widgets = new List<WidgetConfig>(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        Dashboards.Add(dashboard);
        return dashboard.Id;
    }

    public Guid CreateFromTemplate(string templateName, List<string> widgetTypes)
    {
        var id = CreateDashboard(templateName, $"Created from {templateName} template");
        var dashboard = Dashboards.First(d => d.Id == id);

        int col = 0;
        int row = 0;
        foreach (var type in widgetTypes)
        {
            var defaults = WidgetDefaults.Get(type);
            dashboard.Widgets.Add(new WidgetConfig
            {
                Id = Guid.NewGuid(),
                Type = type,
                Title = defaults.Title,
                Width = defaults.Width,
                Height = defaults.Height,
                Column = col,
                Row = row
            });
            col += defaults.Width;
            if (col >= 12) { col = 0; row += defaults.Height; }
        }

        return id;
    }

    public Dashboard? GetDashboard(Guid id) => Dashboards.FirstOrDefault(d => d.Id == id);

    public void AddWidget(Guid dashboardId, string widgetType)
    {
        var dashboard = GetDashboard(dashboardId);
        if (dashboard == null) return;

        var defaults = WidgetDefaults.Get(widgetType);
        dashboard.Widgets.Add(new WidgetConfig
        {
            Id = Guid.NewGuid(),
            Type = widgetType,
            Title = defaults.Title,
            Width = defaults.Width,
            Height = defaults.Height,
            Column = 0,
            Row = dashboard.Widgets.Count > 0 ? dashboard.Widgets.Max(w => w.Row + w.Height) : 0
        });
        dashboard.UpdatedAt = DateTime.UtcNow;
    }

    public void RemoveWidget(Guid dashboardId, Guid widgetId)
    {
        var dashboard = GetDashboard(dashboardId);
        if (dashboard == null) return;

        dashboard.Widgets.RemoveAll(w => w.Id == widgetId);
        dashboard.UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateWidgetTitle(Guid dashboardId, Guid widgetId, string title)
    {
        var widget = GetDashboard(dashboardId)?.Widgets.FirstOrDefault(w => w.Id == widgetId);
        if (widget != null) widget.Title = title;
    }
}

public class Dashboard
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public List<WidgetConfig> Widgets { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class WidgetConfig
{
    public Guid Id { get; set; }
    public string Type { get; set; } = "";
    public string Title { get; set; } = "";
    public int Width { get; set; } = 4;
    public int Height { get; set; } = 3;
    public int Column { get; set; }
    public int Row { get; set; }
    public Dictionary<string, string> Config { get; set; } = new();
}

public static class WidgetDefaults
{
    private static readonly Dictionary<string, (string Title, int Width, int Height)> Defaults = new()
    {
        ["kpi-card"] = ("KPI Card", 3, 2),
        ["incident-feed"] = ("Incident Feed", 6, 4),
        ["incident-chart"] = ("Incident Chart", 6, 4),
        ["incident-table"] = ("Incident Table", 12, 5),
        ["hazard-map"] = ("Hazard Map", 6, 4),
        ["compliance-gauge"] = ("Compliance Status", 4, 3),
        ["severity-gauge"] = ("Severity Breakdown", 4, 3),
        ["training-matrix"] = ("Training Matrix", 8, 5),
        ["equipment-status"] = ("Equipment Status", 6, 4),
        ["alert-panel"] = ("Active Alerts", 6, 4),
        ["action-list"] = ("Outstanding Actions", 6, 4),
        ["inspection-calendar"] = ("Inspection Calendar", 6, 4),
        ["audit-calendar"] = ("Audit Calendar", 6, 4),
        ["findings-chart"] = ("Inspection Findings", 6, 4),
        ["area-scores"] = ("Area Scores", 4, 3),
        ["maintenance-calendar"] = ("Maintenance Schedule", 6, 4),
        ["cert-expiry"] = ("Cert Expiry Alerts", 4, 3),
        ["expiry-alerts"] = ("Training Expiry", 4, 3),
        ["completion-chart"] = ("Completion Rate", 4, 3),
        ["obligation-table"] = ("Obligations", 8, 5),
    };

    public static (string Title, int Width, int Height) Get(string type)
    {
        return Defaults.TryGetValue(type, out var d) ? d : (type, 4, 3);
    }

    public static IEnumerable<string> AllTypes => Defaults.Keys;
}
