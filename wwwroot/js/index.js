// ----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.
// ----------------------------------------------------------------------------

$(function () {
    
    let models = window["powerbi-client"].models;
    let reportContainer = $("#report-container").get(0);

    let reportEmbedData = window.ReportEmbedData;
    
    reportLoadConfig = {
        type: "report",
        id: reportEmbedData.ReportId,
        embedUrl: reportEmbedData.EmbedUrl,
        accessToken: reportEmbedData.EmbedToken,
        tokenType: models.TokenType.Embed
    };
        
    // Embed Power BI report when Access token and Embed URL are available
    // The access token is valid for 60 minutes and will need to be refreshed after that
    let report = powerbi.embed(reportContainer, reportLoadConfig);
    
    // Clear any other loaded handler events
    report.off("loaded");

    // Triggers when a report schema is successfully loaded
    report.on("loaded", function () {
        console.log("Report load successful");
    });

    // Clear any other rendered handler events
    report.off("rendered");

    // Triggers when a report is successfully embedded in UI
    report.on("rendered", function () {
        console.log("Report render successful");
    });

    // Clear any other error handler events
    report.off("error");
    
    // Handle embed errors
    report.on("error", function (event) {
        var errorMsg = event.detail;
    
        // Use errorMsg variable to log error in any destination of choice
        console.error(errorMsg);
        return;
    });
     
});