using SmartSharp.TextBuilder;
using System.Diagnostics;
using System.Text.RegularExpressions;
using FluentAssertions;
using System;
using System.Xml.Linq;
using NUnit.Framework.Interfaces;

namespace TextBuilder_Tester
{
    public class Test_StrPack
    {
        #region ▼ Initialize

        public string text;
        public string html;

        private Stopwatch _stopwatch;
        private long _memoriaAntes;

        [SetUp]
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
                                                <div id='divTemp' class='divPopupVertical popupColorBack_mid popupClosed' data-info='</div>header'>
                                                    <input type='button' class='btnPopupClose popupColorBack_dark' value='x' onclick='subPopup_vertical_OpenClose()'/>
                                                    <span id='divUnitPopup_block_caption' data-info='label_bloco' class='lblTorre_label popupColorBack_mid'>TORRE:</span>
                                                    <div id='divUnitPopup_block' data-info='bloco' class='lblTorre_value popupColorBack_mid'></div>
                                                    <span id='divUnitPopup_group_caption' data-info='label_grupo' class='lblAndar_label popupColorBack_mid'>ANDAR:</span>
                                                    <div id='divUnitPopup_group' data-info='quadra' class='lblAndar_value popupColorBack_mid'></div>
                                                    <span id='divUnitPopup_unit_caption' data-info='label_unidade' class='lblUnidade_label popupColorBack_mid'>APTO:</span>
                                                    <div id='divUnitPopup_unit' data-info='lote' class='lblUnidade_value popupColorBack_mid'></div>
                                                </div>
                                            </div>
                                        divPopupVertical</div>
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
                           with the CNPJ under number 22.724.722/0001-21, headquartered at Avenida José Ferreira 
                           Batista, nº 2281, room 02, Bairro Ipanema, in this City and District of Araçatuba-SP, 
                           represented in this act in accordance with its Articles of Incorporation, in the capacity 
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
                        
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            // Medição de memória antes
            _memoriaAntes = GC.GetTotalMemory(true);

            // Inicia o cronômetro
            _stopwatch = Stopwatch.StartNew();
        }

        [TearDown]
        public void TearDown()
        {
            _stopwatch.Stop();

            long memoriaDepois = GC.GetTotalMemory(true);
            long memoriaConsumida = memoriaDepois - _memoriaAntes;

            //_stopwatch.ElapsedMilliseconds.Should().BeLessThan(500, "o método deve ser rápido")
            //memoriaConsumida.Should().BeLessThan(1024 * 1024, "o método não deve consumir muita memória"); // 1MB

            Console.WriteLine($"Tempo de execução: {_stopwatch.ElapsedMilliseconds} ms");
            Console.WriteLine($"Memória consumida: {memoriaConsumida} bytes");
        }

        #endregion

        #region ▼ Match

        #region ► Litteral Match

        [Test]
        public void Test01_Match()
        {
           StringAndPosition firstMatch = TextBuilder.Match(text,"Marie Doe|John Doe");
            Console.WriteLine(firstMatch.Text + " - " + firstMatch.Position);

            //Duration: 1ms, Memory: 352 bytes

            //RETURN: John Doe - 842
        }

        [Test]
        public void Test02_Match_IgnoreSingleQuotes()
        {
            StringAndPosition firstMatch = TextBuilder.Match(text, "John Doe|Marie Doe", TextOptions.IgnoreInSingleQuotes);
            Console.WriteLine(firstMatch.Text + " - " + firstMatch.Position);

            //Duration: 1ms, Memory: 456 bytes

            //RETURN: Marie Doe - 861
        }

        [Test]
        public void Test02b_Match_IgnoreCase()
        {
            StringAndPosition firstMatch = TextBuilder.Match(text, "john doe|marie doe", TextOptions.IgnoreCase);
            Console.WriteLine(firstMatch.Text + " - " + firstMatch.Position);

            //Duration: 2ms, Memory: 400 bytes

            //RETURN: John Doe - 842
        }

        #endregion

        #region ► Pattern Match

        [Test]
        public void Test03_MatchPattern_PatternInStart()
        {
            StringAndPosition firstMatch = TextBuilder.Match(text, @"*residential");
            Console.WriteLine(firstMatch.Text + " - " + firstMatch.Position);

            //Duration: 1ms, Memory: 7280 bytes


            //RETURN:
            /*
             residential subdivision, named LOTEAMENTO RESIDENCIAL BARCELONA, located 
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
                      - Total Number of Lots: 609. - 1698
             */
        }

        [Test]
        public void Test04_MatchPattern_PatternInMiddle()
        {
            StringAndPosition firstMatch = TextBuilder.Match(text, "Name*Jard*.");
            Console.WriteLine(firstMatch.Text + " - " + firstMatch.Position);

            //Duration: 1ms, Memory: 688 bytes

            //RETURN:
            // Name: Jardim Barcelona -2792 */

        }

        [Test]
        public void Test05_MatchPattern_PatternInEnd()
        {
            StringAndPosition firstMatch = TextBuilder.Match(text, "B.3.*");

            if (firstMatch.Empty)
            {
                Console.WriteLine("Ocorrencia não encontrada!");
            }
            else
            {
                Console.WriteLine(firstMatch.Text + " - " + firstMatch.Position);
            }

            //Duration: 1ms, Memory: 2144 bytes

            //RETURN:
            /*B.3. Deadline for completion of infrastructure works:
                           24 (twenty-four) months, counted from the date of the public launch of the subdivision, 
                           which was carried out on December 5, 2015, and may be anticipated at any time or extended 
                           for a period granted by the Municipality of Araçatuba-SP, under the terms of this instrument.*/

        }

        [Test]
        public void Test06_MatchPattern_MoreThanOnePattern()
        {
            StringAndPosition firstMatch = TextBuilder.Match(text, "Name:*cidade de *.");

            if (firstMatch == null) { Console.WriteLine("Not match found!"); }
            else { Console.WriteLine(firstMatch.Text + " - " + firstMatch.Position); }

            //Duration: 1ms, Memory: 688 bytes

            //RETURN:
            // Name: Jardim Barcelona, cidade de Araraquara/SP. */

        }

        [Test]
        public void Test07_MatchPattern_PatternWithOrCondition()
        {
            StringAndPosition firstMatch = TextBuilder.Match(text, @"married*Marie|John|Jack");
            Console.WriteLine(firstMatch.Text + " - " + firstMatch.Position);

            //Duration: 2ms, Memory: 456 bytes

            //RETURN:
            /*infrastructure - 3521*/
        }

        [Test]
        public void Test07b_MatchPattern_PatternWithOrCondition()
        {
            StringAndPosition firstMatch = TextBuilder.Match(text, @"married*Marie|John|Jack*Mcan|Albert|Towner");
            Console.WriteLine(firstMatch.Text + " - " + firstMatch.Position);

            //Duration: 2ms, Memory: 608 bytes

            //RETURN:
            /*infrastructure - 3521*/
        }

        [Test]
        public void Test07c_MatchPattern_PatternWithOrConditionIgnoreCase()
        {
            StringAndPosition firstMatch = TextBuilder.Match(text, @"married*marie|john|jack", TextOptions.IgnoreCase);
            Console.WriteLine(firstMatch.Text + " - " + firstMatch.Position);

            //Duration: 2ms, Memory: 504 bytes

            //RETURN:
            /*infrastructure - 3521*/
        }

        #endregion

        #region ► Dynamic Match

        [Test]
        public void Test08_MatchDynamic_StartByWord()
        {
            StringAndPosition firstMatch = TextBuilder.MatchDynamic(text, @"\cture");
            Console.WriteLine(firstMatch.Text + " - " + firstMatch.Position);

            //Duration: 2ms, Memory: 504 bytes

            //RETURN:
            /*infrastructure - 3521*/
        }

        [Test]
        public void Test09_MatchDinamic_EndByWord()
        {
            StringAndPosition firstMatch = TextBuilder.MatchDynamic(text, @"infra\");
            Console.WriteLine(firstMatch.Text + " - " + firstMatch.Position);

            //Duration: 2ms, Memory: 504 bytes

            //RETURN:
            /*infrastructure - 3521*/
        }

        [Test]
        public void Test09b_MatchDinamic_EndByWord_IgnoreCase()
        {
            StringAndPosition firstMatch = TextBuilder.MatchDynamic(text, @"inst\", TextOptions.IgnoreCase);
            Console.WriteLine(firstMatch.Text + " - " + firstMatch.Position);

            //Duration: 2ms, Memory: 504 bytes

            //RETURN:
            /*infrastructure - 3521*/
        }

        [Test]
        public void Test10_MatchDynamic_PatternAndAndEndWord()
        {
            StringAndPosition firstMatch = TextBuilder.MatchDynamic(text, @"married to*Sil\");

            if (firstMatch.Empty)
            { Console.WriteLine("Ocorrencia não encontrada!"); }
            else
            { Console.WriteLine(firstMatch.Text + " - " + firstMatch.Position); }

            //Duration: 2ms, Memory: 592 bytes

            //RETURN: <Paulo/SP
        }

        [Test]
        public void Test10b_MatchDynamic_PatternAndAndEndWordIgnoreCase()
        {
            StringAndPosition firstMatch = TextBuilder.MatchDynamic(text, @"married to*sil\", TextOptions.IgnoreCase);

            if (firstMatch.Empty)
            { Console.WriteLine("Ocorrencia não encontrada!"); }
            else
            { Console.WriteLine(firstMatch.Text + " - " + firstMatch.Position); }

            //Duration: 2ms, Memory: 592 bytes

            //RETURN: <Paulo/SP
        }

        [Test]
        public void Test12_MatchDynamic_Number()
        {
            StringAndPosition firstMatch = TextBuilder.MatchDynamic(text, @"#.#.#-#");

            if (firstMatch == null) { Console.WriteLine("Not match found!"); }
            else { Console.WriteLine(firstMatch.Text + " - " + firstMatch.Position); }

            //Duration: 1ms, Memory: 456 bytes

            //RETURN:
            // Name: Jardim Barcelona, cidade de Araraquara/SP. */

        }

        [Test]
        public void Test13_MatchDynamic_NumberAndLitteral()
        {
            StringAndPosition firstMatch = TextBuilder.MatchDynamic(text, @"Area: #,#.#");

            if (firstMatch == null) { Console.WriteLine("Not match found!"); }
            else { Console.WriteLine(firstMatch.Text + " - " + firstMatch.Position); }

            //Duration: 1ms, Memory: 688 bytes

            //RETURN:
            // Area: 315,467.00 - 2872

        }

        [Test]
        public void Test14_MatchDynamic_Letters()
        {
            StringAndPosition firstMatch = TextBuilder.MatchDynamic(text, "@,");

            if (firstMatch == null) { Console.WriteLine("Not match found!"); }
            else { Console.WriteLine(firstMatch.Text + " - " + firstMatch.Position); }

            //Duration: 2ms, Memory: 384 bytes

            //RETURN:
            // LTDA,- 291

        }

        [Test]
        public void Test15_MatchDynamic_WordSeparator()
        {
            StringAndPosition firstMatch = TextBuilder.MatchDynamic(text, "Ipanema_in");

            if (firstMatch.Empty) { Console.WriteLine("Not match found!"); }
            else { Console.WriteLine(firstMatch.Text + " - " + firstMatch.Position); }

            //Duration: 2ms, Memory: 456 bytes

            //RETURN:
            // Ipanema, in - 514

        }

        [Test]
        public void Test16_MatchDynamic_WordSeparator()
        {
            StringAndPosition firstMatch = TextBuilder.MatchDynamic(text, "Table_A");

            if (firstMatch.Empty) { Console.WriteLine("Not match found!"); }
            else { Console.WriteLine(firstMatch.Text + " - " + firstMatch.Position); }

            //Duration: 2ms, Memory: 656 bytes

            //RETURN:
            /* Table

            A - 188 */

        }

        #endregion

        #endregion

        #region ▼ Snippets

        #region ► Snippet

        [Test]
        public void Test13_SnippetFirst()
        {
            string firstMatch = TextBuilder.ExtractFirstSnippet("<div*</div>");
            Console.WriteLine(firstMatch);

            //Duration: 1ms, Memory: 7032 bytes
        }

        [Test]
        public void Test14_SnippetFirstRegex()
        {
            //string firstMatch = Regex.Match(html.SourceText, @"\<div(\s)id\=\'divPopupVertical\'((.|\n|\t)*?)divPopupVertical<\/div\>").Value;
            //Console.WriteLine(firstMatch);

            //Duration: 7ms, Memory: 72144 bytes
        }

        [Test]
        public void Test15_SnippetFirstIdentifiedSnippet()
        {
            string firstMatch = TextBuilder.ExtractFirstSnippet("<div *</div>", "id='divTemp'");
            Console.WriteLine(firstMatch);

            //Duration: 1ms, Memory: 5472 bytes
        }

        [Test]
        public void Test16_SnippetFirstIdentifiedAndConsiderApotrophesContent()
        {
            string firstMatch = TextBuilder.ExtractFirstSnippet("<div *</div>", "id='divTemp'", TextOptions.IgnoreInSingleQuotes);
            Console.WriteLine(firstMatch);

            //Duration: 1ms, Memory: 720 bytes
        }

        #endregion

        #region ► All Snippets

        [Test]
        public void Test17_SnippetsAll()
        {
            string[] firstMatch = TextBuilder.ExtractSnippets("<div *</div>");

            int _index = 1;
            foreach (string s in firstMatch)
            {
                Console.WriteLine($"Snippet #{_index++} -> {Environment.NewLine}{s}");
            }

            //Duration: 1ms, Memory: 352 bytes
        }

        #endregion

        #endregion

    }
}
