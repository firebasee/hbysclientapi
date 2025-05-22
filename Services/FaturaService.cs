using System.Data;
using Ardalis.Result;
using Dapper;
using HBYSClientApi.Interfaces;
using HBYSClientApi.Parameters;

namespace HBYSClientApi.Services;


public class FaturaService(IDbContext dbContext) : IFaturaService
{
    private IDbConnection Connection { get; } = dbContext.GetDbConnection();

    public async Task<Result<string>> BarkodAdetBirYap(string barkod)
    {
        var today = DateTime.Now.ToString("dd-MM-yyyy");
        var query = "UPDATE MEDULA_KAREKOD SET KALAN=1 WHERE BARKOD=:barkod AND SONKULLANIMTARIHI > :today";
        var result= await Connection.ExecuteAsync(query, new { today, barkod });

        return result switch
        {
            > 0 => Result.Success("Barkod miktarı sıfırlandı."),
            _ => Result.Error("Barkod bulunamadı veya sıfırlanamadı.")
        };
    }

    public async Task<Result<string>> BarkodAdetGüncelleSGCCGöre(string barkod)
    {
        var query = """
                    update MEDULA_KAREKOD DD set dd.kalan=0 WHERE   DD.BARKOD=:barkod AND  DD.SN IN (
                    SELECT SSGC.QR_SERI_NO FROM SERVIS_sTOK_GIRIS_CIKIS SSGC WHERE SSGC.YUKLENEN_SIRANO IN (
                    SELECT A.YUKLENEN_SIRANO FROM SERVIS_sTOK_GIRIS_CIKIS A WHERE A.QR_BARKOD=:barkod AND A.DEVIR_GIRIS_CIKIS='C' AND A.YUKLENEN_SIRANO IN 
                    (SELECT YH.SIRA_NO FROM YUKLENEN_HIZMETLER YH
                    LEFT JOIN FATURA_BILGILERI FB ON FB.ONKAYIT_SIRANO=YH.ONKAYIT_SIRANO
                    WHERE FB.FATURA_REFNO IS NOT NULL
                    )))
            """;
        try
        {
            await Connection.ExecuteAsync(query, new { barkod });
            return Result.Success("Barkod adeti ssgc'ye göre güncellendi.");
        }
        catch (Exception ex)
        {
            return Result.Error($"Bir hata oluştu: {ex.Message}");
        }
    }

    public async Task<Result<string>> BarkodAdetGüncelleYuklenenHizmetlereGöre(string barkod)
    {
         var query = """                        
                update MEDULA_KAREKOD DD set dd.kalan=0 WHERE   DD.BARKOD=:barkod AND  DD.YUKLENENSIRANO IN (
                SELECT MK.YUKLENENSIRANO FROM MEDULA_KAREKOD MK WHERE MK.BARKOD=:barkod  and MK.YUKLENENSIRANO IN (
                (SELECT YH.SIRA_NO FROM YUKLENEN_HIZMETLER YH
                LEFT JOIN FATURA_BILGILERI FB ON FB.ONKAYIT_SIRANO=YH.ONKAYIT_SIRANO
                WHERE FB.FATURA_REFNO IS NOT NULL
                ))) 
            """;
        try
        {
            await Connection.ExecuteAsync(query, new { barkod });
            return Result.Success("Barkod adeti yüklenen hizmetlere göre güncellendi.");
        }
        catch (Exception ex)
        {
            return Result.Error($"Bir hata oluştu: {ex.Message}");
        }
    }

    public async Task<Result<string>> BarkodAdetGüncelleMedulaKarekodSiranoyaGöre(string barkod)
    {
        var query = """
                    update MEDULA_KAREKOD DD set dd.kalan=0 WHERE   DD.BARKOD=:barkod AND  DD.SN IN (
                    SELECT SSGC.QR_SERI_NO FROM SERVIS_sTOK_GIRIS_CIKIS SSGC WHERE SSGC.YUKLENEN_SIRANO IN (
                    SELECT A.YUKLENEN_SIRANO FROM SERVIS_sTOK_GIRIS_CIKIS A WHERE A.QR_BARKOD=:barkod AND A.DEVIR_GIRIS_CIKIS='C' AND A.YUKLENEN_SIRANO IN 
                    (SELECT YH.SIRA_NO FROM YUKLENEN_HIZMETLER YH
                    LEFT JOIN FATURA_BILGILERI FB ON FB.ONKAYIT_SIRANO=YH.ONKAYIT_SIRANO
                    WHERE FB.FATURA_REFNO IS NOT NULL
                    )))
            """;
        try
        {
            await Connection.ExecuteAsync(query, new { barkod });
            return Result.Success("Barkod adeti medula karekod siranoya göre güncellendi.");
        }
        catch (Exception ex)
        {
            return Result.Error($"Bir hata oluştu: {ex.Message}");
        }
    }

    public async Task<Result<string>> SariAlanaÇevir(ProtocolOrIcmalNo icmalNo)
    {
        var query = """
        UPDATE ONKAYIT S SET S.DOSYA_IZLEME_DURUMU=1 WHERE S.SIRA_NO IN (SELECT A.ONKAYIT_SIRANO FROM YATIS_POLIKLINIK A WHERE A.ICMAL_NO=:ICMAL_NO);
        """;
        var result = await Connection.ExecuteAsync(query, new { ICMAL_NO = icmalNo.Value });

        return result switch
        {
            > 0 => Result.Success("İcmal No'su sarı alana çevrildi."),
            _ => Result.Error("İcmal No'su bulunamadı veya çevrilemedi.")
        };
    }

    public async Task<Result<string>> YeşilAlanSil(ProtocolOrIcmalNo icmalNo)
    {
        var query = """
            DELETE FROM YUKLENEN_HIZMETLER WHERE SIRA_NO IN 
            (SELECT S.SIRA_NO FROM YUKLENEN_HIZMETLER S WHERE S.ONKAYIT_SIRANO IN (SELECT A.ONKAYIT_SIRANO FROM YATIS_POLIKLINIK A WHERE A.ICMAL_NO=:ICMAL_NO)
             AND S.TETKIK_KODU=7625);
        """;
        var result = await Connection.ExecuteAsync(query, new { ICMAL_NO = icmalNo.Value });
        
        return result switch
        {
            > 0 => Result.Success("İcmal No'su yeşil alandan silindi."),
            _ => Result.Error("İcmal No'su bulunamadı veya silinemedi.")
        };
    }

    public async Task<Result<string>> SariAlanEkle(ProtocolOrIcmalNo icmalNo)
    {
        var query = """
                BEGIN
                    FOR REC IN (SELECT S.YAT_POL, S.ONKAYIT_SIRANO, S.PROTOKOL_NO, S.DOKTOR_KODU, S.I_DOKTOR_KODU, S.ISLEM_YAPAN, S.ISLEM_TARIHI
                                FROM YUKLENEN_HIZMETLER S
                                WHERE S.ONKAYIT_SIRANO IN (SELECT A.ONKAYIT_SIRANO FROM YATIS_POLIKLINIK A WHERE A.ICMAL_NO = :ICMAL_NO)
                                AND S.TETKIK_KODU = 215)
                        LOOP
                            INSERT INTO HBYS.YUKLENEN_HIZMETLER (SIRA_NO, YAT_POL, PROTOKOL_NO, ISLEM_TARIHI, ISLEM_YAPAN, ADET,
                                                                GRUP_KODU, TETKIK_KODU, FIYAT, KURUM_HASTA, GELIR_KODU, DOKTOR_KODU,
                                                                AKTIF, IND_TUTAR, LIS_GONDERILDI, I_DOKTOR_KODU, DS_PUAN,
                                                                E_FATURA_TUTARI, ONKAYIT_SIRANO, HASTA_PAYI, KDV_ORANI,
                                                                MEDULA_ISLEM_SIRANO, MUADIL_SIRANO, YIL, ISTEM_NO, OTURUM_KODU,
                                                                VETA_ID, HIZMETREFNO, HASTA_PAYI2, MESAI_TURU, URUN_GIRIS_SIRANO,
                                                                MUADIL_DS_SIRANO, TAKIP_NO, UYGULAMA_SIRANO, MUADIL_KODU,
                                                                MEDULA_OZEL_DURUM, RBYS_WEB_REFNO, RBYS_WEB_DURUM, RBYS_WEB_ISTEMNO,
                                                                ALIS_FIYATI, ACILIYET_DURUMU, BAGLI_YUKLENEN_SIRANO, MESAI_TURU_SECIMI,
                                                                SN_OP_PROTOKOL_ID, KULLANICI_KURUM_HASTA, VEZNE_AYRINTI_SIRANO,
                                                                TAHSILAT_TURU, UYGULAYAN_HEMSIRE, EGITIM_HIZMETI, LAB_PANEL_ID,
                                                                ENJ_PANS_SIRANO, LBYS_MUKERRER_TETKIK_ID, OTO_HIZMET, KRMDLASTUPDATE,
                                                                BIRIM_FIYAT, TAMAMLAYICI_FIYAT, OZEL_PROVIZYON, HL7_PLACER_ID,
                                                                HL7_FILLER_ID, BAP_HIZMETI, CHSS_KOD, IHSS_KOD, UCR_SONRA_EKLENEN,
                                                                ODA_NO, HASTA_ONAYI, KRMDLASTUPOTURUMID, KRMDINSOTURUMID, KRMDINSDATE,
                                                                BAP_PROJE_NO, ICMAL_NO, FATURA_TARIHI, RAPOR_TAKIP_NO, SERVIS_KODU,
                                                                USS_SONUCKODU, USS_SON_GONDERIMTAR, MDU_HASTA_PAYI,
                                                                MDU_VEZNE_AYRINTI_SIRANO, MDU_INDIRIM, PUAN_TARIHI, PUAN_ONAY_TARIHI,
                                                                GERCEKLESME_TARIHI, USS_PAKET_KODU, MDU_HIZMETI, KURAL_KONTROL_EDILDI,
                                                                ESLIK_EDEN, USSLASTUPDATE, USS_268_SONUCKODU, USS_268_SON_GONDERIMTAR,
                                                                UYGULAMA_TARIHI, RANDEVU_TARIHI, USS_409_SONUCKODU,
                                                                USS_409_SON_GONDERIMTAR, MEDULA_ENABIZ_ISLEMKODU,
                                                                MEDULA_ENABIZ_ISLEMTARIHI, USS_700_SONUCKODU, USS_700_SON_GONDERIMTAR,
                                                                DS_KODU, USS_502_SONUCKODU, USS_502_SON_GONDERIMTAR,
                                                                MUAYENE_HIZMET_TURU, ALIS_FIYATI_KDVSIZ, HIZMET_ACIL_TURU,
                                                                KRMDVERSIYON, PAKET_KODU, ISERVIS_KODU_DOSYADAN_AL,
                                                                SAGLIKNETE_GITMESIN, LIMIT_DAHILINDE, SAG_SOL, BIRLIKTE_HIZMET,
                                                                MEDULA_DURUMU, UCR_IKINCI_BTMR, PAKET_HIZMET_ID, ATIPUANUPDATE, UBB,
                                                                CZMDOKTOR, ACIKLAMA, TUP_BEBEK_SEANSI, GECICI_INDIRIM_YAPILDI,
                                                                GECICI_INDIRIM_UYGULAYAN, PAKET_SIRA_NO, PROTOKOL_YENILEME_HIZMETI,
                                                                DS_PUAN_HAM, MEDULA_KAREKOD_SIRANO, USS_112_SON_GONDERIMTAR,
                                                                USS_112_SONUCKODU, KDS_SONUCKODU, SABIT_KURUM_ID,
                                                                HIZMET_PARAMETRE_DISI, FATURA_EDILEMESIN, USS_277_SONUCKODU,
                                                                USS_277_SON_GONDERIMTAR)
                            VALUES (YUKLENEN_SIRANO.nextval, REC.YAT_POL, REC.PROTOKOL_NO, REC.ISLEM_TARIHI, REC.ISLEM_YAPAN, 20, 336, 12410, 0,
                                    'K', 1537,
                                    REC.DOKTOR_KODU, 257, 0, null, REC.I_DOKTOR_KODU, 26, null, REC.ONKAYIT_SIRANO, 0, null, null, null,
                                    null, null, null, null, null,
                                    0, 0, null, 0, null, null, 661333, null, null, null, null, 0, 1, null, 0, null, null, null, 0, 0,
                                    'F', 0, 0, 0, 1, null, 0, 0, null, null, null, 'F', null, null,
                                    'F', 0, null, null, null, null, null, null, null, null,
                                    null, null, null, null, null, null, null, null, null, null,
                                    null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
                                    null, 0, 0, null, 2, null, null, null, null, null, null, null, 'F', null, null, null, null, null, 0,
                                    null, null, 0, 'F', 26, null, null, null, null, null, null, null, null, null);
                        end loop;
                    COMMIT;
                end;
        """;
        var result = await Connection.ExecuteAsync(query, new { ICMAL_NO = icmalNo.Value });

        return result switch
        {
            > 0 => Result.Success("İcmal No'su yeşil alana eklendi."),
            _ => Result.Error("İcmal No'su bulunamadı veya eklenemedi.")
        };
    }

    public async Task<Result<string>> HizmetSil(int id)
    {
        var query = "DELETE FROM YUKLENEN_HIZMETLER WHERE SIRA_NO = :id";
        var result = await Connection.ExecuteAsync(query, new { id });

        return result switch
        {
            > 0 => Result.Success("Hizmet hasta dosyasından silindi."),
            _ => Result.Error("Hizmet bulunamadı veya silinemedi.")
        };
    }

    public async Task<Result<string>> HizmetAdetGüncelle(int id, decimal adet)
    {
        var query = "UPDATE YUKLENEN_HIZMETLER SET ADET = :adet WHERE SIRA_NO = :id";
        var result = await Connection.ExecuteAsync(query, new { adet, id });

        return result switch
        {
            > 0 => Result.Success("Hizmet miktarı güncellendi."),
            _ => Result.Error("Hizmet bulunamadı veya güncellenemedi.")
        };
    }
}
