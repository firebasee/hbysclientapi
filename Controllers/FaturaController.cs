using Ardalis.Result.AspNetCore;
using HBYSClientApi.Interfaces;
using HBYSClientApi.Parameters;
using Microsoft.AspNetCore.Mvc;

namespace HBYSClientApi.Controllers;

[Route("api/[controller]")]
[ApiController]
public class FaturaController(IFaturaService faturaService) : ControllerBase
{
    [HttpPost("barkod-adet-bir-yap")]
    public async Task<ActionResult<string>> BarkodAdetBirYap([FromQuery] string barkod)
    {
        var result = await faturaService.BarkodAdetBirYap(barkod);
        return result.ToActionResult(this);
    }

    [HttpPost("barkod-adet-guncelle-ssgc")]
    public async Task<ActionResult<string>> BarkodAdetGuncelleSGCC([FromQuery] string barkod)
    {
        var result = await faturaService.BarkodAdetGüncelleSGCCGöre(barkod);
        return result.ToActionResult(this);
    }

    [HttpPost("barkod-adet-guncelle-yuklenen-hizmetler")]
    public async Task<ActionResult<string>> BarkodAdetGuncelleYuklenenHizmetler([FromQuery] string barkod)
    {
        var result = await faturaService.BarkodAdetGüncelleYuklenenHizmetlereGöre(barkod);
        return result.ToActionResult(this);
    }

    [HttpPost("barkod-adet-guncelle-medula-karekod")]
    public async Task<ActionResult<string>> BarkodAdetGuncelleMedulaKarekod([FromQuery] string barkod)
    {
        var result = await faturaService.BarkodAdetGüncelleMedulaKarekodSiranoyaGöre(barkod);
        return result.ToActionResult(this);
    }

    [HttpPost("sari-alana-cevir")]
    public async Task<ActionResult<string>> SariAlanaCevir([FromBody] ProtocolOrIcmalNo icmalNo)
    {
        var result = await faturaService.SariAlanaÇevir(icmalNo);
        return result.ToActionResult(this);
    }

    [HttpPost("yesil-alan-sil")]
    public async Task<ActionResult<string>> YesilAlanSil([FromBody] ProtocolOrIcmalNo icmalNo)
    {
        var result = await faturaService.YeşilAlanSil(icmalNo);
        return result.ToActionResult(this);
    }

    [HttpPost("sari-alan-ekle")]
    public async Task<ActionResult<string>> SariAlanEkle([FromBody] ProtocolOrIcmalNo icmalNo)
    {
        var result = await faturaService.SariAlanEkle(icmalNo);
        return result.ToActionResult(this);
    }

    [HttpDelete("hizmet-sil/{id}")]
    public async Task<ActionResult<string>> HizmetSil(int id)
    {
        var result = await faturaService.HizmetSil(id);
        return result.ToActionResult(this);
    }

    [HttpPut("hizmet-adet-guncelle/{id}")]
    public async Task<ActionResult<string>> HizmetAdetGuncelle(int id, [FromQuery] decimal adet)
    {
        var result = await faturaService.HizmetAdetGüncelle(id, adet);
        return result.ToActionResult(this);
    }
}