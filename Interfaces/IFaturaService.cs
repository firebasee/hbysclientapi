using Ardalis.Result;
using HBYSClientApi.Parameters;

namespace HBYSClientApi.Interfaces;

public interface IFaturaService
{
    Task<Result<string>> BarkodAdetBirYap(string barkod);
    Task<Result<string>> BarkodAdetGüncelleSGCCGöre(string barkod);
    Task<Result<string>> BarkodAdetGüncelleYuklenenHizmetlereGöre(string barkod);
    Task<Result<string>> BarkodAdetGüncelleMedulaKarekodSiranoyaGöre(string barkod);
    Task<Result<string>> SariAlanaÇevir(ProtocolOrIcmalNo icmalNo);
    Task<Result<string>> SariAlanEkle(ProtocolOrIcmalNo icmalNo);
    Task<Result<string>> YeşilAlanSil(ProtocolOrIcmalNo icmalNo);
    Task<Result<string>> HizmetSil(int id);
    Task<Result<string>> HizmetAdetGüncelle(int id, decimal adet);
}