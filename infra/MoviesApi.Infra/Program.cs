using MoviesApi.Infra;
using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;
using Pulumi.AzureNative.Web;
using Pulumi.AzureNative.Web.Inputs;
using AppInsights = Pulumi.AzureNative.Insights;
using OperationalInsights = Pulumi.AzureNative.OperationalInsights;

return await Pulumi.Deployment.RunAsync(() =>
{
    var resourceGroup = new ResourceGroup("rg-moviesapi");

    var logAnalyticsWorkspace = new OperationalInsights.Workspace("log-moviesapi", new()
    {
        ResourceGroupName = resourceGroup.Name,
        RetentionInDays = 30,
        Sku = new OperationalInsights.Inputs.WorkspaceSkuArgs {
            Name = OperationalInsights.WorkspaceSkuNameEnum.Free
        },
    });

    var applicationInsights = new AppInsights.Component("appi-moviesapi", new()
    {
        ResourceGroupName = resourceGroup.Name,
        ApplicationType = AppInsights.ApplicationType.Web,
        IngestionMode = AppInsights.IngestionMode.LogAnalytics,
        WorkspaceResourceId = logAnalyticsWorkspace.Id,
        Kind = "web",
    });

    var storageAccount = new StorageAccount("stmoviesapi", new()
    {
        ResourceGroupName = resourceGroup.Name,
        Kind = Kind.StorageV2,
        Sku = new SkuArgs
        {
            Name = SkuName.Standard_LRS
        },
    });

    var table = new Table("Movies", new()
    {
        ResourceGroupName = resourceGroup.Name,
        AccountName = storageAccount.Name,
    });

    var appServicePlan = new AppServicePlan("asp-moviesapi", new()
    {
        ResourceGroupName = resourceGroup.Name,
        Kind = "App",
        Sku = new SkuDescriptionArgs
        {
            Tier = "Free",
            Name = "F1",
        },
    });

    var app = new WebApp($"app-moviesapi", new WebAppArgs
    {
        ResourceGroupName = resourceGroup.Name,
        ServerFarmId = appServicePlan.Id,
        SiteConfig = new SiteConfigArgs
        {
            AppSettings = new NameValuePairArgs[]
            {
                new ()
                {
                    Name = "StorageConnectionString",
                    Value = OutputHelpers.StorageConnectionString(storageAccount, resourceGroup),
                },
                new ()
                {
                    Name = "APPLICATIONINSIGHTS_CONNECTION_STRING",
                    Value = Output.Format($"InstrumentationKey={applicationInsights.InstrumentationKey}"),
                },
            },
        },
    });

    return new Dictionary<string, object?>
    {
        ["AppName"] = app.Name
    };
});