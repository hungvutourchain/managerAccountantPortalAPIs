using System;
using System.Collections.Generic;
using System.Linq;

namespace B2BAdmin.ApiDocument.Domains.Models
{
    public static class LanguageCodeExtension
    {
        public static LanguageCode GetDefaultLanguageId(this List<LanguageCode> languageCodes)
        {
            var defaultCodes = new List<string> { "en", "vi" };
            var langCode = defaultCodes.FirstOrDefault(x => languageCodes.Any(y => y.Code == x));
            var lang = languageCodes.FirstOrDefault(x => x.Code == langCode)
                ?? languageCodes[0];

            return lang;

            /*
            getLanguageCodeIdDefault(lsLangCode: LanguageCodeInfo[]) { // copy from hotel_tour_portal: edit.component.ts
                let defaultCodes = ["en", "vi"];
                let lang: LanguageCodeInfo;

                defaultCodes.some(c => {
                    lang = lsLangCode.find(l => l.code === c);
                    if (lang)
                        return true;
                    return false;
                });

                return lang ? lang._id : lsLangCode[0]._id;
            }
            */
        }
    }
}
