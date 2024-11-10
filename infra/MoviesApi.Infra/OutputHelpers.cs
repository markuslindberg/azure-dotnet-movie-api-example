using Pulumi;
using Pulumi.AzureNative.Storage;

namespace MoviesApi.Infra;

public static class OutputHelpers
{
  public static Output<string> StorageConnectionString(StorageAccount account, Output<string> resourceGroupName)
  {
    return Output.Tuple(account.Name, resourceGroupName).Apply(t =>
    {
      (string accountName, string resourceGroupName) = t;

      var accountKeys = ListStorageAccountKeys.InvokeAsync(new()
      {
        ResourceGroupName = resourceGroupName,
        AccountName = accountName
      });

      return Output.CreateSecret($"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accountKeys.Result.Keys[0].Value};EndpointSuffix=core.windows.net");
    });
  }
}