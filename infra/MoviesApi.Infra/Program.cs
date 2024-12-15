using MoviesApi.Infra;
using Pulumi;
using Pulumi.AzureAD;
using Pulumi.AzureNative.Resources;
using Pulumi.AzureNative.Storage;
using Pulumi.AzureNative.Web;
using Pulumi.AzureNative.Web.Inputs;
using AppInsights = Pulumi.AzureNative.Insights;

return await Pulumi.Deployment.RunAsync(() =>
{
    var azureConfig = new Pulumi.Config("azure-native");
    var tenantId = azureConfig.Require("tenantId");

    var resourceGroupName = Output.Create(GetResourceGroup.InvokeAsync(new GetResourceGroupArgs
    {
        ResourceGroupName = "rg-moviesapi"
    })).Apply(x => x.Name);

    var appRegistration = new Application("appreg-moviesapi", new ApplicationArgs
    {
        DisplayName = "Movies Api",
        Api = new Pulumi.AzureAD.Inputs.ApplicationApiArgs
        {
            Oauth2PermissionScopes = new[]
            {
                new Pulumi.AzureAD.Inputs.ApplicationApiOauth2PermissionScopeArgs
                {
                    Id = new Pulumi.Random.RandomUuid("appreg-moviesapi-scope-id").Id,
                    Type = "User",
                    Value = "user_impersonation",
                    AdminConsentDescription = "Allow the application access on behalf of the signed-in user.",
                    AdminConsentDisplayName = "Access Movies Api",
                    UserConsentDescription = "Allow the application access on your behalf.",
                    UserConsentDisplayName = "Access Movies Api",
                    Enabled = true,
                }
            }
        }
    });

    var appIdentifierUri = new ApplicationIdentifierUri("appreg-moviesapi-identifier-uri", new ApplicationIdentifierUriArgs
    {
        ApplicationId = appRegistration.Id,
        IdentifierUri = Output.Format($"api://{appRegistration.ClientId}")
    });

    var appSecret = new ApplicationPassword("appreg-moviesapi-secret", new ApplicationPasswordArgs
    {
        ApplicationId = appRegistration.Id,
        EndDate = DateTimeOffset.Parse("2025-01-01").AddDays(90).ToString("o"),
    });

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
        Sku = new Pulumi.AzureNative.Storage.Inputs.SkuArgs
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
        HttpsOnly = true,
        SiteConfig = new SiteConfigArgs
        {
            MinTlsVersion = "1.2",
            AppSettings = [
                new NameValuePairArgs()
                {
                    Name = "StorageConnectionString",
                    Value = OutputHelpers.StorageConnectionString(storageAccount, resourceGroupName)
                },
                new NameValuePairArgs()
                {
                    Name = "APPLICATIONINSIGHTS_CONNECTION_STRING",
                    Value = Output.Format($"InstrumentationKey={applicationInsights.InstrumentationKey}")
                },
                new NameValuePairArgs()
                {
                    Name = "WEBSITE_AUTH_AAD_ALLOWED_TENANTS",
                    Value = tenantId
                },
                new NameValuePairArgs()
                {
                    Name = "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET",
                    Value = appSecret.Value
                },
            ],
        },
    });

    var authSettings = new WebAppAuthSettingsV2("app-moviesapi-auth", new WebAppAuthSettingsV2Args
    {
        Name = app.Name,
        ResourceGroupName = resourceGroupName,
        GlobalValidation = new GlobalValidationArgs
        {
            RequireAuthentication = true,
            UnauthenticatedClientAction = UnauthenticatedClientActionV2.Return401,
        },
        Platform = new AuthPlatformArgs
        {
            Enabled = true,
        },
        IdentityProviders = new IdentityProvidersArgs
        {
            AzureActiveDirectory = new AzureActiveDirectoryArgs
            {
                Enabled = true,
                Registration = new AzureActiveDirectoryRegistrationArgs
                {
                    ClientId = appRegistration.ClientId,
                    ClientSecretSettingName = "MICROSOFT_PROVIDER_AUTHENTICATION_SECRET",
                    OpenIdIssuer = $"https://login.microsoftonline.com/{tenantId}/v2.0"
                },
                Validation = new AzureActiveDirectoryValidationArgs
                {
                    AllowedAudiences = 
                    {
                        Output.Format($"api://{appRegistration.ClientId}"),
                        appRegistration.ClientId
                    }
                }
            }
        },
    });

    return new Dictionary<string, object?>
    {
        ["AppName"] = app.Name,
        ["AppUrl"] = Output.Format($"https://{app.DefaultHostName}")
    };
});