using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.PowerBI.Api;
using Microsoft.PowerBI.Api.Models;
using Microsoft.Rest;
using AppOwnsData.Models;
using Microsoft.Identity.Client;
using System.Data;

namespace AppOwnsData.Services {

  public class PowerBiServiceApi {

    private Guid tenantId { get; }
    private string clientId { get; }
    private string clientSecret { get; }
    private Guid workspaceId { get; }
    private Guid reportId { get; }
    private string powerbiServiceApiRoot { get; }
    private string powerBiServiceApiResourceId { get; }
    public string[] powerbiDefaultScope { get; }

    public PowerBiServiceApi(IConfiguration configuration) {

      this.tenantId = Guid.Parse(configuration["AzureAd:TenantId"]);
      this.clientId = configuration["AzureAd:ClientId"];
      this.clientSecret = configuration["AzureAd:ClientSecret"];

      this.workspaceId = Guid.Parse(configuration["PowerBi:workspaceId"]);
      this.reportId = Guid.Parse(configuration["PowerBi:reportId"]);

      this.powerbiServiceApiRoot = configuration["PowerBi:PowerBiServiceApiRoot"];
      this.powerBiServiceApiResourceId = configuration["PowerBi:PowerBiServiceApiResourceId"];
      this.powerbiDefaultScope = new string[] { this.powerBiServiceApiResourceId + "/.default" };
    }

    public string GetAppOnlyAccessToken() {

      string tenantSpecificAuthority = "https://login.microsoftonline.com/" + tenantId;

      var appConfidential =
          ConfidentialClientApplicationBuilder.Create(clientId)
            .WithClientSecret(clientSecret)
            .WithAuthority(tenantSpecificAuthority)
            .Build();

      var authResult = appConfidential.AcquireTokenForClient(powerbiDefaultScope).ExecuteAsync().Result;
      return authResult.AccessToken;

    }

    public PowerBIClient GetPowerBiClient() {
      var tokenCredentials = new TokenCredentials(GetAppOnlyAccessToken(), "Bearer");
      return new PowerBIClient(new Uri(powerbiServiceApiRoot), tokenCredentials);
    }

    public async Task<ReportEmbedData> GetReportEmbeddingData() {

      PowerBIClient pbiClient = GetPowerBiClient();

      var report = await pbiClient.Reports.GetReportInGroupAsync(workspaceId, reportId);
      var datasetId = report.DatasetId;


      IList<GenerateTokenRequestV2Dataset> datasetRequests = new List<GenerateTokenRequestV2Dataset>();
      datasetRequests.Add(new GenerateTokenRequestV2Dataset(datasetId));

      IList<GenerateTokenRequestV2Report> reportRequests = new List<GenerateTokenRequestV2Report>();
      reportRequests.Add(new GenerateTokenRequestV2Report(reportId, allowEdit: true));

      GenerateTokenRequestV2 tokenRequest =
        new GenerateTokenRequestV2 {
          Datasets = datasetRequests,
          Reports = reportRequests
        };

      var embedTokenResponse = pbiClient.EmbedToken.GenerateToken(tokenRequest);

      // return report embedding data to caller
      return new ReportEmbedData {
        ReportId = reportId.ToString(),
        EmbedUrl = report.EmbedUrl,
        Token = embedTokenResponse.Token
      };
    }

    public async Task<ReportEmbedData> GetReportEmbeddingDataWithRls(string UserName, string RlsRole, string CustomData = "") {

      PowerBIClient pbiClient = GetPowerBiClient();

      var report = await pbiClient.Reports.GetReportInGroupAsync(workspaceId, reportId);
      var datasetId = report.DatasetId;


      IList<GenerateTokenRequestV2Dataset> datasetRequests = new List<GenerateTokenRequestV2Dataset>();
      datasetRequests.Add(new GenerateTokenRequestV2Dataset(datasetId));

      IList<GenerateTokenRequestV2Report> reportRequests = new List<GenerateTokenRequestV2Report>();
      reportRequests.Add(new GenerateTokenRequestV2Report(reportId, allowEdit: true));

      var datasetList = new List<string>() { report.DatasetId };
      var rlsRoles = new string[] { RlsRole };

      IList<EffectiveIdentity> effectiveIdentities =
        new List<EffectiveIdentity> {
          new EffectiveIdentity(UserName, datasets:datasetList, roles: rlsRoles, customData: CustomData ) };


      GenerateTokenRequestV2 tokenRequest =
        new GenerateTokenRequestV2 {
          Datasets = datasetRequests,
          Reports = reportRequests,
          Identities = effectiveIdentities
        };

      var embedTokenResponse = pbiClient.EmbedToken.GenerateToken(tokenRequest);

      // return report embedding data to caller
      return new ReportEmbedData {
        ReportId = reportId.ToString(),
        EmbedUrl = report.EmbedUrl,
        Token = embedTokenResponse.Token
      };
    }

  }
}
