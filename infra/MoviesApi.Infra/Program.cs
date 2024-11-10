using MoviesApi.Infra;
using Pulumi;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Storage.Inputs;
using Pulumi.AzureNative.Web;
using Pulumi.AzureNative.Web.Inputs;
using AppInsights = Pulumi.AzureNative.Insights;

return await Pulumi.Deployment.RunAsync(() =>
{
    var resourceGroupName = Output.Create(GetResourceGroup.InvokeAsync(new GetResourceGroupArgs
    {
        ResourceGroupName = "rg-moviesapi"
    })).Apply(x => x.Name);

    var applicationInsights = new AppInsights.Component("appi-moviesapi", new()
    {
        ResourceGroupName = resourceGroupName,
        ApplicationType = AppInsights.ApplicationType.Web,
        IngestionMode = AppInsights.IngestionMode.ApplicationInsights,
        Kind = "web",
    });

    var storageAccount = new StorageAccount("stmoviesapi", new()
    {
        ResourceGroupName = resourceGroupName,
        Kind = Kind.StorageV2,
        Sku = new SkuArgs
        {
            Name = SkuName.Standard_LRS
        },
    });

    var table = new Table("Movies", new()
    {
        ResourceGroupName = resourceGroupName,
        AccountName = storageAccount.Name,
    });

    var appServicePlan = new AppServicePlan("asp-moviesapi", new()
    {
        ResourceGroupName = resourceGroupName,
        Kind = "App",
        Sku = new SkuDescriptionArgs
        {
            Tier = "Free",
            Name = "F1",
        },
    });

    var app = new WebApp($"app-moviesapi", new WebAppArgs
    {
        ResourceGroupName = resourceGroupName,
        ServerFarmId = appServicePlan.Id,
        SiteConfig = new SiteConfigArgs
        {
            AppSettings = new NameValuePairArgs[]
            {
                new ()
                {
                    Name = "StorageConnectionString",
                    Value = OutputHelpers.StorageConnectionString(storageAccount, resourceGroupName),
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