namespace MyHerbalife3.Ordering.Providers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;

    /// <summary>
    /// Simple class to convert HTML text to plain text for SKU description.
    /// </summary>
    public static class HTMLHelper
    {
        public static string ToText(string html)
        {
            string plainText = html;

            // Replace some html tags
            plainText = Regex.Replace(plainText, @"<( )*br( )*>", "\r", RegexOptions.IgnoreCase);
            plainText = Regex.Replace(plainText, @"<( )*li( )*>", "\r", RegexOptions.IgnoreCase);
            plainText = Regex.Replace(plainText, @"<( )*div([^>])*>", "\r", RegexOptions.IgnoreCase);
            plainText = Regex.Replace(plainText, @"<( )*p([^>])*>", "\r", RegexOptions.IgnoreCase);
            plainText = Regex.Replace(plainText, @"<[^>]*>", string.Empty, RegexOptions.IgnoreCase);

            // Replace some special characters for punctuation
            plainText = Regex.Replace(plainText, @"&quot;", "\"", RegexOptions.IgnoreCase);
            plainText = Regex.Replace(plainText, @"&ldquo;", "“", RegexOptions.IgnoreCase);
            plainText = Regex.Replace(plainText, @"&rdquo;", "”", RegexOptions.IgnoreCase);
            plainText = Regex.Replace(plainText, @"&lsquo;", "‘", RegexOptions.IgnoreCase);
            plainText = Regex.Replace(plainText, @"&rsquo;", "’", RegexOptions.IgnoreCase);
            plainText = Regex.Replace(plainText, @"&laquo;", "«", RegexOptions.IgnoreCase);
            plainText = Regex.Replace(plainText, @"&raquo;", "»", RegexOptions.IgnoreCase);

            plainText = Regex.Replace(plainText, @"&aacute;", "á");
            plainText = Regex.Replace(plainText, @"&eacute;", "é");
            plainText = Regex.Replace(plainText, @"&iacute;", "í");
            plainText = Regex.Replace(plainText, @"&oacute;", "ó");
            plainText = Regex.Replace(plainText, @"&uacute;", "ú");

            plainText = Regex.Replace(plainText, @"&Aacute;", "Á");
            plainText = Regex.Replace(plainText, @"&Eacute;", "É");
            plainText = Regex.Replace(plainText, @"&Iacute;", "Í");
            plainText = Regex.Replace(plainText, @"&Oacute;", "Ó");
            plainText = Regex.Replace(plainText, @"&Uacute;", "Ú");

            plainText = Regex.Replace(plainText, @"&Auml;", "Ä");
            plainText = Regex.Replace(plainText, @"&Euml;", "Ë");
            plainText = Regex.Replace(plainText, @"&Iuml;", "Ï");
            plainText = Regex.Replace(plainText, @"&Ouml;", "Ö");
            plainText = Regex.Replace(plainText, @"&Uuml;", "Ü");

            plainText = Regex.Replace(plainText, @"&auml;", "ä");
            plainText = Regex.Replace(plainText, @"&euml;", "ë");
            plainText = Regex.Replace(plainText, @"&iuml;", "ï");
            plainText = Regex.Replace(plainText, @"&ouml;", "ö");
            plainText = Regex.Replace(plainText, @"&uuml;", "ü");

            plainText = Regex.Replace(plainText, @"&Acirc;", "Â");
            plainText = Regex.Replace(plainText, @"&Ecirc;", "Ê");
            plainText = Regex.Replace(plainText, @"&Icirc;", "Î");
            plainText = Regex.Replace(plainText, @"&Ocirc;", "Ô");
            plainText = Regex.Replace(plainText, @"&Ucirc;", "Û");

            plainText = Regex.Replace(plainText, @"&acirc;", "â");
            plainText = Regex.Replace(plainText, @"&ecirc;", "ê");
            plainText = Regex.Replace(plainText, @"&icirc;", "î");
            plainText = Regex.Replace(plainText, @"&ocirc;", "ô");
            plainText = Regex.Replace(plainText, @"&ucirc;", "û");

            plainText = Regex.Replace(plainText, @"&Atilde;", "Ã");
            plainText = Regex.Replace(plainText, @"&Ntilde;", "Ñ");
            plainText = Regex.Replace(plainText, @"&Otilde;", "Õ");
            plainText = Regex.Replace(plainText, @"&atilde;", "ã");
            plainText = Regex.Replace(plainText, @"&ntilde;", "ñ");
            plainText = Regex.Replace(plainText, @"&otilde;", "õ");
            plainText = Regex.Replace(plainText, @"&Oslash;", "Ø");
            plainText = Regex.Replace(plainText, @"&oslash;", "ø");
            plainText = Regex.Replace(plainText, @"&ETH;", "Ð");
            plainText = Regex.Replace(plainText, @"&eth;", "ð");
            plainText = Regex.Replace(plainText, @"&szlig;", "ß");

            plainText = Regex.Replace(plainText, @"&aring;", "å");
            plainText = Regex.Replace(plainText, @"&Aring;", "Å");
            plainText = Regex.Replace(plainText, @"&Ccedil;", "Ç");
            plainText = Regex.Replace(plainText, @"&ccedil;", "ç");
            plainText = Regex.Replace(plainText, @"&Yacute;", "Ý");
            plainText = Regex.Replace(plainText, @"&yacute;", "ý");
            plainText = Regex.Replace(plainText, @"&yuml;", "ÿ");
            plainText = Regex.Replace(plainText, @"&THORN;", "Þ");
            plainText = Regex.Replace(plainText, @"&thorn;", "þ");
            plainText = Regex.Replace(plainText, @"&AElig;", "Æ");
            plainText = Regex.Replace(plainText, @"&aelig;", "æ");

            plainText = Regex.Replace(plainText, @"&Agrave;", "À");
            plainText = Regex.Replace(plainText, @"&Egrave;", "È");
            plainText = Regex.Replace(plainText, @"&Igrave;", "Ì");
            plainText = Regex.Replace(plainText, @"&Ograve;", "Ò");
            plainText = Regex.Replace(plainText, @"&Ugrave;", "Ù");

            plainText = Regex.Replace(plainText, @"&agrave;", "à");
            plainText = Regex.Replace(plainText, @"&egrave;", "è");
            plainText = Regex.Replace(plainText, @"&igrave;", "ì");
            plainText = Regex.Replace(plainText, @"&ograve;", "ò");
            plainText = Regex.Replace(plainText, @"&ugrave;", "ù");

            plainText = Regex.Replace(plainText, @"&nbsp;", string.Empty, RegexOptions.IgnoreCase);

            // if Text contain more characters
            if (plainText.Contains("&"))
            {
                //Replace Greece characters for symbols
                plainText = Regex.Replace(plainText, @"&Alpha;", "Α");
                plainText = Regex.Replace(plainText, @"&Beta;", "Β");
                plainText = Regex.Replace(plainText, @"&Gamma;", "Γ");
                plainText = Regex.Replace(plainText, @"&Delta;", "Δ");
                plainText = Regex.Replace(plainText, @"&Épsilon;", "Ε");
                plainText = Regex.Replace(plainText, @"&Epsilon;", "Ε");
                plainText = Regex.Replace(plainText, @"&Zeta;", "Ζ");
                plainText = Regex.Replace(plainText, @"&Eta;", "Η");
                plainText = Regex.Replace(plainText, @"&Theta;", "Θ");
                plainText = Regex.Replace(plainText, @"&Iota;", "Ι");
                plainText = Regex.Replace(plainText, @"&Kappa;", "Κ");
                plainText = Regex.Replace(plainText, @"&Lambda;", "Λ");
                plainText = Regex.Replace(plainText, @"&Mu;", "Μ");
                plainText = Regex.Replace(plainText, @"&Nu;", "Ν");
                plainText = Regex.Replace(plainText, @"&Xi;", "Ξ");
                plainText = Regex.Replace(plainText, @"&Ómicron;", "Ο");
                plainText = Regex.Replace(plainText, @"&Omicron;", "Ο");
                plainText = Regex.Replace(plainText, @"&Pi;", "Π");
                plainText = Regex.Replace(plainText, @"&Ro;", "Ρ");
                plainText = Regex.Replace(plainText, @"&Rho;", "Ρ");
                plainText = Regex.Replace(plainText, @"&Sigma;", "Σ");
                plainText = Regex.Replace(plainText, @"&Tau;", "Τ");
                plainText = Regex.Replace(plainText, @"&Ípsilon;", "Υ");
                plainText = Regex.Replace(plainText, @"&Ipsilon;", "Υ");
                plainText = Regex.Replace(plainText, @"&Upsilon;", "Υ");
                plainText = Regex.Replace(plainText, @"&Phi;", "Φ");
                plainText = Regex.Replace(plainText, @"&Chi;", "Χ");
                plainText = Regex.Replace(plainText, @"&Psi;", "Ψ");
                plainText = Regex.Replace(plainText, @"&Omega;", "Ω");

                plainText = Regex.Replace(plainText, @"&alpha;", "α");
                plainText = Regex.Replace(plainText, @"&beta;", "β");
                plainText = Regex.Replace(plainText, @"&gamma;", "γ");
                plainText = Regex.Replace(plainText, @"&delta;", "δ");
                plainText = Regex.Replace(plainText, @"&épsilon;", "ε");
                plainText = Regex.Replace(plainText, @"&epsilon;", "ε");
                plainText = Regex.Replace(plainText, @"&zeta;", "ζ");
                plainText = Regex.Replace(plainText, @"&eta;", "η");
                plainText = Regex.Replace(plainText, @"&theta;", "θ");
                plainText = Regex.Replace(plainText, @"&iota;", "ι");
                plainText = Regex.Replace(plainText, @"&kappa;", "κ");
                plainText = Regex.Replace(plainText, @"&lambda;", "λ");
                plainText = Regex.Replace(plainText, @"&mu;", "μ");
                plainText = Regex.Replace(plainText, @"&nu;", "ν");
                plainText = Regex.Replace(plainText, @"&xi;", "ξ");
                plainText = Regex.Replace(plainText, @"&ómicron;", "ο");
                plainText = Regex.Replace(plainText, @"&omicron;", "ο");
                plainText = Regex.Replace(plainText, @"&pi;", "π");
                plainText = Regex.Replace(plainText, @"&rho;", "ρ");
                plainText = Regex.Replace(plainText, @"&sigmaf;", "ς");
                plainText = Regex.Replace(plainText, @"&sigma;", "σ");
                plainText = Regex.Replace(plainText, @"&tau;", "τ");
                plainText = Regex.Replace(plainText, @"&ípsilon;", "υ");
                plainText = Regex.Replace(plainText, @"&ipsilon;", "υ");
                plainText = Regex.Replace(plainText, @"&upsilon;", "υ");
                plainText = Regex.Replace(plainText, @"&phi;", "φ");
                plainText = Regex.Replace(plainText, @"&chi;", "χ");
                plainText = Regex.Replace(plainText, @"&psi;", "ψ");
                plainText = Regex.Replace(plainText, @"&omega;", "ω");
            }
            // Replace some special characters for symbols
            plainText = Regex.Replace(plainText, @"&trade;", "™", RegexOptions.IgnoreCase);
            plainText = Regex.Replace(plainText, @"&gt;", ">", RegexOptions.IgnoreCase);
            plainText = Regex.Replace(plainText, @"&lt;", "<", RegexOptions.IgnoreCase);
            plainText = Regex.Replace(plainText, @"&copy;", "©", RegexOptions.IgnoreCase);
            plainText = Regex.Replace(plainText, @"&reg;", "®", RegexOptions.IgnoreCase);
            plainText = Regex.Replace(plainText, @"&amp;", "&", RegexOptions.IgnoreCase);

            plainText = Regex.Replace(plainText, @"( )+", " ");
            while (plainText.Contains("\r\r"))
            {
                plainText = plainText.Replace("\r\r", "\r");
            }

            return plainText;
        }
    }
}