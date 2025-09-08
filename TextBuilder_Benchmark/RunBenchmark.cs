using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using SmartSharp.TextBuilder;
using System.Text.RegularExpressions;
using static System.Net.Mime.MediaTypeNames;

[MemoryDiagnoser]
public class ComparadorDeDesempenho
{
    private string html="";
    private string text="";

    [GlobalSetup]
    public void Setup()
    {
        html = @"<head>
                    <meta charset='UTF-8'>
                    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                    <title></title>
                    <link href='../fonts.css' rel=\'stylesheet'/>
                    <link href='../styles/vertical/popup_master.css?v=4' rel='stylesheet'/>
                    <link href='../styles/vertical/popup_visitor.css?v=6' rel='stylesheet'/>
                    <script src='../scripts/vertical/scriptCheckbutton.js'></script>
                </head>
                <body style='width:1920px; height:1080px'>
                  <section id='popup'>
                    <div id='divPopupVertical' class='divPopupVertical popupColorBack_light popupClosed'>teste
                       <div id='divUnitPopup' class='divPopupVertical popupColorBack_mid popupOpened' data-unidade=''>                
                          <div id='divTemp' class='divPopupVertical popupColorBack_mid popupClosed' data-info='header'>
                             <input type='button' class='btnPopupClose popupColorBack_dark' value='x' onclick='subPopup_vertical_OpenClose()'/>
                             <span id='divUnitPopup_block_caption' data-info='label_bloco' class='lblTorre_label popupColorBack_mid'>TORRE:</span>
                             <div id='divUnitPopup_block' data-info='bloco' class='lblTorre_value popupColorBack_mid'>divTemp</div>
                             <span id='divUnitPopup_group_caption' data-info='label_grupo' class='lblAndar_label popupColorBack_mid'>ANDAR:</span>
                             <div id='divUnitPopup_group' data-info='quadra' class='lblAndar_value popupColorBack_mid'></div>
                             <span id='divUnitPopup_unit_caption' data-info='label_unidade' class='lblUnidade_label popupColorBack_mid'>APTO:</span>
                             <div id='divUnitPopup_unit' data-info='lote' class='lblUnidade_value popupColorBack_mid'></div>
                         </div>
                       </div>
                     </div>
                     <div class='divPlant popupColorBack_mid'>
                       <div class='divMetragem'>
                         <span data-info='metragem' data-form='popup' data-field='metragem' id='lblMetragem_value'>0</span><br>m²
                       </div>
                       <div style='height:20px; top:-2px; left:26px;' class='divPlantVerticalLine'></div>
                       <div style='width:26px; top:40px; left:-2px;' class='divPlantHorizontalLine'></div>
                       <div style='height:12px; top:40px; left:24px;' class='divPlantVerticalLine'></div>
                       <div style='height:16px; top:74px; left:24px;' class='divPlantVerticalLine'></div>
                       <div style='height:8px; top:-2px; left:84px;' class='divPlantVerticalLine'></div>
                       <div id='divValorFinal' style='width:32px; top:44px; left:84px;' class='divPlantHorizontalLine'></div>
                   </div>
                </section>";

        text = @"PRIVATE INSTRUMENT OF PROMISE OF PURCHASE AND SALE OF PROPERTY
                      
                      SUBJECT OF SUBDIVISION
                      
                      Summary Table

                      A. PARTIES
                      A.1. LOTEAMENTO RESIDENCIAL BARCELONA LTDA, a private law company, duly registered 
                           with CNPJ under number 74.099.252/0001-56, headquartered at Avenida José Ferreira 
                           Batista react, nº2281, action room 02, Bairro Ipanema, in this City and District of Araçatuba-SP, 
                           represented in this act in accordance with its Articles of incorporation, in the capacity 
                           of Owner and Developer, hereinafter simply referred to as SELLER.
                      A.2. PROMISING BUYER(S), married with 'John Doe Towner' , Marie Doe Towner is Brazilian national, broker, married, registered under 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to John Doe Silva, registered under CPF number 012.869.980-93, 
                           RG number 102.456.543-2 SSP, in partial community property regime, Brazilian national, lawyer, 
                           hereinafter simply referred to as BUYER.

                      B. THE SUBDIVISION:

                      The type of property is a residential subdivision, named LOTEAMENTO RESIDENCIAL BARCELONA, located 
                      in Araçatuba, State of São Paulo, registered under property record nº 100.314 at the Real Estate 
                      Registry Office of the District of Araçatuba – State of São Paulo.

                      B.1. Subdivision Description:
                           The LOTEAMENTO RESIDENCIAL BARCELONA will be developed according to the specifications 
                           contained in the Descriptive Report, copies of which will be archived along with the 
                           registration process of the development.
                           The BUYER is fully and unequivocally aware that the information contained in the Conceptual 
                           Plan is merely illustrative and may be modified according to the needs of the project and 
                           the interest of the SELLER and/or determinations from the competent public authorities.

                      B.2. Basic Characteristics of the Subdivision:

                      - Name: Jardim Barcelona, cidade de Araraquara/SP.
                      - Total Area: 315,467.00 m².
                      - Area allocated for Residential Lots: 136,705.37 m².
                      - Area allocated for Commercial Lots: 3,031.19 m².
                      - Leisure System: 27,381.80 m².
                      - Area allocated for the Entrance Gate: 704.85 m².
                      - Green Areas: 40,860.29 m².
                      - Total Road System: 91,579.16 m².
                      - Institutional Areas: 15,909.19 m².
                      - Total Number of Lots: 609.
                      - Residential Lots: 608.
                      - Commercial Lots: 01.

                      B.3. Deadline for completion of infrastructure works:
                           24 (twenty-four) months, counted from the date of the public launch of the subdivision, 
                           which was carried out on December 5, 2015, and may be anticipated at any time or extended 
                           for a period granted by the Municipality of Araçatuba-SP, under the terms of this instrument.";
    }

    [Benchmark]
    public bool UsandoTextBuilder()
    {
        #region + Simple match with OR

        //TextMatcher builder = new TextMatcher(text);
        //StringAndPosition result = builder.Match("Marie Doe|Jane Doe|Jack|John Doe");

        #endregion

        #region + Match with ignore apostrophes

        //TextMatcher builder = new TextMatcher(text);
        //builder.IgnoreCharsInQuotes = true;
        //StringAndPosition result = builder.Match("Marie Doe|Jane Doe|Jack|John Doe"); // Método estático

        #endregion

        #region + Simple match ignoreCase

        //TextMatcher builder = new TextMatcher(text);
        //builder.CaseSensitive = false;
        //StringAndPosition result = TextBuilder.Match(text, "Marie Doe|Jane Doe|Jack|John Doe"); // Método estático

        #endregion

        #region + Wildcard match

        //TextMatcher builder = new TextMatcher(text);
        //StringAndPosition result = TextBuilder.Match(text, "*residential");

        //StringAndPosition result = TextBuilder.Match(text, "J*ner");

        //StringAndPosition result = TextBuilder.Match(text, "B.3.*");

        //StringAndPosition result = TextBuilder.Match(text, "Name:*cidade de *.");

        //StringAndPosition result = TextBuilder.Match(text, "email*@hotmail.com|@gmail.com|@yahoo.com");

        //StringAndPosition result = TextBuilder.Match(text,"married*Marie|John|Jack* Mcan| Albert| Towner");

        //StringAndPosition result = TextBuilder.Match(text,"_react");

        //string result = TextBuilder.ReplaceFirst(text, "Marie Doe Towner", "Jene Doe Sanders");

        //string result = TextBuilder.ReplaceFirst(text, "Marie*Towner", "Jene Doe Sanders");

        string result = TextBuilder.Translate(text, "Doe;married;,", "Silva;Divorced;<o>");

        #endregion

        //if (result.Empty)
        //    return false;
        //return true;

        if (result =="")
            return false;
        return true;
    }

    [Benchmark]
    public bool UsandoRegex()
    {
        #region + Simple match with OR
        //return Regex.Match(text, "Marie Doe|Jane Doe|Jack|John Doe").Success;
        #endregion

        #region + Match with ignore apostrophes

        //return Regex.Match(text, @"'[^']*'|(?<!')\bMarie Doe|Jane Doe|Jack|John Doe\b(?!')").Success;

        #endregion

        #region + Match with ignore apostrophes

        //return Regex.Match(text, "Marie Doe|Jane Doe|Jack|John Doe", RegexOptions.IgnoreCase).Success;

        #endregion

        #region + Match pattern

        //string test = Regex.Match(text, "(.*?)residential").Value;

        //string test = Regex.Match(text, "j(.*?)ner").Value;

        //string test = Regex.Match(text, @"B\.3\.(.*?)").Value;

        //string test = Regex.Match(text, @"Name:(.*?)cidade de (.*?)\.").Value;

        //string test = Regex.Match(text, @"email(.*?)(@hotmail.com|@gmail.com|@yahoo.com)").Value;

        //string test = Regex.Match(text, @"married(.*?)(Marie|John|(Jack(.*?)Mcan)| Albert| Towner)").Value;

        //string test = Regex.Match(text, @"(\s)react").Value;

        //string test = Regex.Replace(text, @"Marie Doe Towner", "Jene Doe Sanders");

        //string test = Regex.Replace(text, @"Marie(.*?)Towner", "Jene Doe Sanders");

        string test = Regex.Replace(text, @"Doe|married|,", "Silva");

        if (test == null) { return false; }
        return true;

        #endregion

    }
}