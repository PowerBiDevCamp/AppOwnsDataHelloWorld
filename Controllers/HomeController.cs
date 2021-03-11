using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using AppOwnsData.Services;
using Microsoft.AspNetCore.Hosting;

namespace AppOwnsData.Controllers {

  public class HomeController : Controller {

    private PowerBiServiceApi powerBiServiceApi;

    public HomeController(PowerBiServiceApi powerBiServiceApi) {
      this.powerBiServiceApi = powerBiServiceApi;
    }

    public async Task<IActionResult> Index() {
      var viewModel = await this.powerBiServiceApi.GetReportEmbeddingData(); ;
      return View(viewModel);
    }

  }
}
