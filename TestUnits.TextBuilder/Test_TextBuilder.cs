using BenchmarkDotNet.Toolchains.Roslyn;
using SmartSharp.TextBuilder;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace TextBuilder_Tester
{
    public class Test_StrPack
    {
        #region ▼ Initialize

        public string text;
        public string html;
        public string tinyHtml;
        public string tinyText;
        public string testText;

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

            tinyHtml = 
@"
<section id='popup'>
   <div id='divPopupVertical' class='divPopupVertical popupColorBack_light popupClosed'>teste
     <div id='divUnitPopup' class='divPopupVertical popupColorBack_mid popupOpened' data-unidade=''>                
       <div id='divTemp' class='divPopupVertical popupColorBack_mid popupClosed' data-info='header'>
         <span id='divUnitPopup_block_caption' data-info='label_bloco' class='lblTorre_label popupColorBack_mid'>TORRE:</span>
         <div id='divUnitPopup_block' data-info='bloco' class='lblTorre_value popupColorBack_mid'>divTemp</div>
         <span id='divUnitPopup_group_caption' data-info='label_grupo' class='lblAndar_label popupColorBack_mid'>ANDAR:</span>
         <div id='divUnitPopup_group' data-info='quadra' class='lblAndar_value popupColorBack_mid'></div>
         <span id='divUnitPopup_unit_caption' data-info='label_unidade' class='lblUnidade_label popupColorBack_mid'>APTO:</span>
         <div id='divUnitPopup_unit' data-info='lote' class='lblUnidade_value popupColorBack_mid'></div>
       </div>
	 </div>  
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

            tinyText = @"A. PARTIES
                      A.1. SUBDIVISION LOTEAMENTO RESIDENCIAL BARCELONA LTDA, online a private reaction law company, duly reactor registered 
                           with CNPJ under number 22.724.722/0001-21, headquartered at Avenida José Ferreira 
                           Batista react, nº2281, action room 02, Ipanema neighborhood, in Bentonville City and District of Araçatuba-SP, 
                           represented in this act in accordance and notion with its Articles of Incorporation, in the capacity 
                           of Louis Doe Towner and Developer, hereinafter simply referred to as SELLER.
                      B.2. PROMISING BUYER(S), married with 'John Doe Towner' , Marie Doe Towner is Brazilian national, broker, married, registered under 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021, mobile (11) 98134-0021, and 
                           email marie@gmail.com; married to John Doe Towner, registered under CPF number 012.869.980-93, 
                           RG number 102.456.543-2 SSP, in partial community property regime, Brazilian national, lawyer, 
                           hereinafter simply referred to as BUYER.";

            testText = @"A.2. PROMISING BUYER(S), married with 'John Doe Towner' , Marie Doe Towner is Brazilian national, broker, 
                           married, registered under action and react law company, duly act registered 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021.";

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

        #region ▼ Words

        #region ► Litteral Match

        [Test]
        public void Test01_Match()
        {
            TextMatcher builder = new TextMatcher(text);
            StringAndPosition firstMatch = builder.Match("Marie Doe|Jane Doe|Jack|John Doe");

            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else
            { Console.WriteLine(firstMatch.Text); }

            //Duration: 2ms, Memory: 352 bytes

            //RETURN: John Doe - 842
        }
                
        [Test]
        public void Test01a_Match_IgnoreSingleQuotes()
        {
            StringAndPosition firstMatch;
            using (var builder = new TextMatcher(text))
            {
                builder.EnableIgnoreCharsInQuotes();
                firstMatch = builder.Match("John Doe|Marie Doe");
            }

            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else
            { Console.WriteLine(firstMatch.Text); }

            //Duration: 3ms, Memory: 352 bytes

            //RETURN: Marie Doe - 861
        }
                
        [Test]
        public void Test01b_Match_IgnoreCase()
        {
            StringAndPosition firstMatch = new TextMatcher(text){ CaseSensitive = false }.Match("john doe|marie doe");

            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else
            { Console.WriteLine(firstMatch.Text); }

            //Duration: 2ms, Memory: 352 bytes

            //RETURN: John Doe
        }

        #endregion

        #region ► Pattern Match

        [Test]
        public void Test02_MatchPattern_PatternInStart()
        {
            TextMatcher builder = new TextMatcher(text);
            StringAndPosition firstMatch = builder.Match("*residential");

            //StringAndPosition firstMatch = builder.Match("*residential");
            Console.WriteLine(firstMatch.Text);

            //Duration: 2ms, Memory: 7296 bytes


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
        public void Test02a_MatchPattern_PatternInMiddle()
        {
            TextMatcher builder = new TextMatcher(text);
            StringAndPosition firstMatch = builder.Match("J*ner");

            if (firstMatch.Empty) { Console.WriteLine("Match not found!"); }
            else { Console.WriteLine(firstMatch.Text); }

            //Duration: 2ms, Memory: 456 bytes

            //RETURN:
            //John Doe Towner*/

        }
                
        [Test]
        public void Test02b_MatchPattern_PatternInEnd()
        {
            TextMatcher builder = new TextMatcher(text);
            StringAndPosition firstMatch = builder.Match("B.3.*");

            if (firstMatch.Empty)
            {
                Console.WriteLine("Ocorrencia não encontrada!");
            }
            else
            {
                Console.WriteLine(firstMatch.Text);
            }

            //Duration: 3ms, Memory: 2112 bytes

            //RETURN:
            /*B.3. Deadline for completion of infrastructure works:
                           24 (twenty-four) months, counted from the date of the public launch of the subdivision, 
                           which was carried out on December 5, 2015, and may be anticipated at any time or extended 
                           for a period granted by the Municipality of Araçatuba-SP, under the terms of this instrument.*/

        }

        [Test]
        public void Test02c_MatchPattern_PatternInMiddleAndEnd()
        {

            TextMatcher builder = new TextMatcher(text);
            StringAndPosition firstMatch = builder.Match("Name*Jard*.");
            Console.WriteLine(firstMatch.Text);

            //Duration: 2ms, Memory: 656 bytes

            //RETURN:
            // Name: Jardim Barcelona, cidade de Araraquara/SP. - 2792 */

        }

        [Test]
        public void Test02d_MatchPattern_MoreThanOnePattern()
        {
            TextMatcher builder = new TextMatcher(text);
            StringAndPosition firstMatch = builder.Match("Name:*cidade de *.");

            if (firstMatch.Empty) { Console.WriteLine("Not match found!"); }
            else { Console.WriteLine(firstMatch.Text); }

            //Duration: 2ms, Memory: 656 bytes

            //RETURN:
            // Name: Jardim Barcelona, cidade de Araraquara/SP. */

        }

        [Test]
        public void Test02e_MatchPattern_PatternWithOrCondition_wrong_OR_use()
        {
            /*Wrong 'OR" condition use. Did put '@' after wildcard and before 'OR' char condition. 
             * So the '@' is alone and it is a more one option in condition and not a char linked 
             * of email servers of the 'OR' condition.*/

            TextMatcher builder = new TextMatcher(text);
            StringAndPosition firstMatch = builder.Match("email*@|hotmail.com|gmail.com|yahoo.com");
            if (firstMatch.Empty) { Console.WriteLine("Not match found!"); }
            else { Console.WriteLine(firstMatch.Text); }

            //Duration: 2ms, Memory: 352 bytes

            //RETURN:
            /*email marie@*/
        }
                
        [Test]
        public void Test02f_MatchPattern_PatternWithOrCondition()
        {
            /*This test is using right the 'OR' condition.*/
            TextMatcher builder = new TextMatcher(text);
            StringAndPosition firstMatch = builder.Match("email*@hotmail.com|@gmail.com|@yahoo.com");
            if (firstMatch.Empty) { Console.WriteLine("Not match found!"); }
            else { Console.WriteLine(firstMatch.Text); }

            //Duration: 2ms, Memory: 456 bytes

            //RETURN:
            /*email marie@gmail.com*/
        }

        [Test]
        public void Test02g_MatchPattern_PatternWithOrCondition()
        {
            /*Is sure exist a more better form of do This pattern*/
            TextMatcher builder = new TextMatcher(text);
            StringAndPosition firstMatch = builder.Match("email*.com|.com.br");
            if (firstMatch.Empty) { Console.WriteLine("Not match found!"); }
            else { Console.WriteLine(firstMatch.Text); }

            //Duration: 2ms, Memory: 456 bytes

            //RETURN:
            /*email marie@gmail.com*/
        }

        [Test]
        public void Test02h_MatchPattern_PatternWithOrCondition()
        {
            TextMatcher builder = new TextMatcher(text);
            StringAndPosition firstMatch = builder.Match("married*Marie|John|Jack* Mcan| Albert| Towner");
            if (firstMatch.Empty) { Console.WriteLine("Not match found!"); }
            else { Console.WriteLine(firstMatch.Text); }

            //Duration: 2ms, Memory: 456 bytes

            //RETURN:
            /*married with 'John Doe Towner*/
        }

        [Test]
        public void Test02i_MatchPattern_PatternWithOrConditionIgnoreCase()
        {
            StringAndPosition firstMatch;
            using (var builder = new TextMatcher(text))
            {
                builder.DisableCaseSensitive();
                firstMatch = builder.Match("married*marie|john|jack");
            }

            if (firstMatch.Empty) { Console.WriteLine("Not match found!"); }
            else { Console.WriteLine(firstMatch.Text); }

            //Duration: 3ms, Memory: 456 bytes

            //RETURN:
            /*married with 'John*/
        }

        #endregion

        #region ► Dynamic Match

        [Test]
        public void Test03a_Match_SeparatorWord_in_start()
        {
            TextMatcher builder = new TextMatcher(tinyText);
            StringAndPosition firstMatch = builder.Match("_react");

            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else { Console.WriteLine("[" + firstMatch.Text + "]"); }

            //Duration: 2ms, Memory: 384 bytes

            //RETURN: [ act] 
            /*Return occurrence is the apart of "react" word in tinyText. 
            Phrase in text: "Batista re[act], nº 2281, action room 02, Bairro Ipanema, in this City and District 
            of Araçatuba-SP"*/
        }

        [Test]
        public void Test03b_Match_SeparatorWord_in_start_CutStartChar()
        {
            StringAndPosition firstMatch;
            using (var builder = new TextMatcher(tinyText))
            {
                builder.setSupressCharsInStart(1); 
                firstMatch = builder.Match("_react");
            }

            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else { Console.WriteLine("[" + firstMatch.Text + "] - " + firstMatch.Position); }

            //Duration: 2ms, Memory: 384 bytes

            //RETURN: [act] - 161
            /*The same of previous exemple, but since user just want return the word, it used a 'startIndexReturn' paramater and with it can remove a 
             frist char of occurence return that this case is a space.*/
        }

        [Test]
        public void Test03c_Match_SeparatorWord_separator_in_end()
        {
            TextMatcher builder = new TextMatcher(tinyText);
            StringAndPosition firstMatch = builder.Match("act_");

            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else { Console.WriteLine("[" + firstMatch.Text + "]"); }

            //Duration: 2ms, Memory: 384 bytes

            //RETURN: [act,]
            /*Return occurrence is a part of "action" word in tinyText. 
             *Phrase in text: "Avenida José Ferreira Batista react, nº 2281, [act]ion room 02"*/
        }

        [Test]
        public void Test03d_Match_SeparatorWord_separator_end_CutEndChar()
        {
            StringAndPosition firstMatch;
            using (var builder = new TextMatcher(tinyText))
            {
                builder.setSupressCharsInEnd(1);
                firstMatch = builder.Match("act_");
            }

            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else { Console.WriteLine("[" + firstMatch.Text + "]"); }

            //Duration: 2ms, Memory: 352 bytes

            //RETURN:[act]
            //Return is exactly word "act" in tinyText.
            //Phrase in text: "represented in this [act] in accordance and notion with its Articles"
        }

        [Test]
        public void Test03d_Match_SeparatorWord_separator_start_end()
        {
            TextMatcher builder = new TextMatcher(tinyText);
            StringAndPosition firstMatch = builder.Match("_act_");

            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else { Console.WriteLine("[" + firstMatch.Text + "]"); }

            //Duration: 2ms, Memory: 352 bytes

            //RETURN: [ act ]
            //Same of previous exemple, but used 'startIndexReturn' and 'endCutLenReturn' to remove start and end
            //separator of occurence return"
        }

        [Test]
        public void Test03e_Match_SeparatorWord_separator_start_end_CutStartEndChar()
        {
            StringAndPosition firstMatch;
            using (var builder = new TextMatcher(tinyText))
            {
                builder.setSupressCharsInEnd(1);
                builder.setSupressCharsInStart(1);
                firstMatch = builder.Match("_act_");
            }

            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else { Console.WriteLine("[" + firstMatch.Text + "]"); }

            //Duration: 2ms, Memory: 352 bytes

            //RETURN: [act]
            //Same of previous exemple, but used 'startIndexReturn' and 'endCutLenReturn' to remove start and end
            //separator of occurence return"
        }

        [Test]
        public void Test04_MatchDynamic_startCompleteWord()
        {
            TextMatcher builder = new TextMatcher(tinyText);
            StringAndPosition firstMatch = builder.Match("~on");

            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else { Console.WriteLine( firstMatch.Text ); }

            //Duration: 2ms, Memory: 352 bytes

            //RETURN:
            /*reaction*/
        }

        [Test]
        public void Test04a_MatchDynamic_EndCompleteWord()
        {
            TextMatcher builder = new TextMatcher(tinyText);
            StringAndPosition firstMatch = builder.Match("on~");

            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else { Console.WriteLine(firstMatch.Text); }

            //Duration: 2ms, Memory: 352 bytes

            //RETURN:
            /*online*/
        }

        [Test]
        public void Test04b_MatchDynamic_startCompleteWord_ignoreCase()
        {
            StringAndPosition firstMatch;
            using (var builder = new TextMatcher(tinyText))
            {
                builder.DisableCaseSensitive();
                firstMatch = builder.Match("~on");
            }

            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else { Console.WriteLine(firstMatch.Text); }

            //Duration: 2ms, Memory: 352 bytes

            //RETURN:
            /*SUBDIVISION*/
        }

        [Test]
        public void Test04c_MatchDynamic_StartAndEndCompleteWord()
        {
            TextMatcher builder = new TextMatcher(tinyText);
            StringAndPosition firstMatch = builder.Match("~on~");

            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else { Console.WriteLine(firstMatch.Text); }

            //Duration: 3ms, Memory: 352 bytes

            //RETURN:
            /*Bentonville*/
        }

        [Test]
        public void Test05_MatchDynamic_NumbersChar()
        {
            TextMatcher builder = new TextMatcher(tinyText);
            StringAndPosition firstMatch = builder.Match("#.#.#/#-#");

            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else { Console.WriteLine(firstMatch.Text); }

            //Duration: 2ms, Memory: 352 bytes

            //RETURN:
            /*22.724.722/0001-21*/
        }

        [Test]
        public void Test05a_MatchDynamic_StartNumbersChar()
        {
            TextMatcher builder = new TextMatcher(tinyText);
            StringAndPosition firstMatch = builder.Match("nº#");

            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else { Console.WriteLine(firstMatch.Text); }

            //Duration: 2ms, Memory: 352 bytes

            //RETURN:
            /*nº2281*/
        }

        [Test]
        public void Test05b_MatchDynamic_EndNumbersChar()
        {
            TextMatcher builder = new TextMatcher(tinyText);
            StringAndPosition firstMatch = builder.Match("B.#.");

            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else { Console.WriteLine(firstMatch.Text); }

            //Duration: 2ms, Memory: 352 bytes

            //RETURN:
            /*B.2.*/
        }

        [Test]
        public void Test06_MatchDynamic_DynmaicsAndWildcard()
        {
            TextMatcher builder = new TextMatcher(text);
            StringAndPosition firstMatch = builder.Match("Road System: #*m²");

            if (firstMatch.Empty)
            { Console.WriteLine("Match not found!"); }
            else { Console.WriteLine(firstMatch.Text); }

            //Duration: 3ms, Memory: 592 bytes

            //RETURN:
            /*Road System: 91,579.16 m² - 3264*/
        }

        #endregion      

        #region ▼ Insert

        [Test]
        public void Test15_Insert()
        {
            string firstMatch = TextBuilder.Insert(testText,"the client ", 75);
            Console.WriteLine(firstMatch);

            //Duration: 1ms, Memory: 6216 bytes

            /*RETURN: 
             A.2. PROMISING BUYER(S), married with 'John Doe Towner' , Marie Doe Towner the client is Brazilian national, broker, 
                           married, registered under action and react law company, duly act registered 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021.
             */
        }

        [Test]
        public void Test15a_InsertBeforeFirst()
        {
            string firstMatch = TextBuilder.InsertBeforeFirst(testText, "the client ", "Marie");
            Console.WriteLine(firstMatch);

            //Duration: 1ms, Memory: 6216 bytes

            /*RETURN: 
             A.2. PROMISING BUYER(S), married with 'John Doe Towner' , the client Marie Doe Towner is Brazilian national, broker, 
                           married, registered under action and react law company, duly act registered 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021.
             */
        }

        [Test]
        public void Test15b_InsertAfterFirst()
        {
            string firstMatch = TextBuilder.InsertAfterFirst(testText, " the client", "Marie");
            Console.WriteLine(firstMatch);

            //Duration: 1ms, Memory: 6216 bytes

            /*RETURN: 
             A.2. PROMISING BUYER(S), married with 'John Doe Towner' , Marie the client Doe Towner is Brazilian national, broker, 
                           married, registered under action and react law company, duly act registered 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021.
             */
        }

        [Test]
        public void Test15c_InsertBefore()
        {
            string firstMatch = TextBuilder.InsertBefore(testText, "<o>", ",");
            Console.WriteLine(firstMatch);

            //Duration: 1ms, Memory: 6216 bytes

            /*RETURN: 
             A.2. PROMISING BUYER(S)<o>, married with 'John Doe Towner' <o>, Marie Doe Towner is Brazilian national<o>, broker<o>, 
                           married<o>, registered under action and react law company<o>, duly act registered 
                           CPF number 675.019.610-18<o>, RG number 23.300.225-3 SSP<o>, residing at Rua XV de Novembro<o>, 3456<o>, 
                           Apt. 21 C<o>, Centro district<o>, postal code 04021-002<o>, located in the city of São Paulo/SP<o>, 
                           providing contact information: phone (11) 34134-0021.
             */
        }

        [Test]
        public void Test15d_InsertAfter()
        {
            string firstMatch = TextBuilder.InsertAfter(testText, "<o>", ",");
            Console.WriteLine(firstMatch);

            //Duration: 1ms, Memory: 6216 bytes

            /*RETURN: 
             A.2. PROMISING BUYER(S)<o>, married with 'John Doe Towner' <o>, Marie Doe Towner is Brazilian national<o>, broker<o>, 
                           married<o>, registered under action and react law company<o>, duly act registered 
                           CPF number 675.019.610-18<o>, RG number 23.300.225-3 SSP<o>, residing at Rua XV de Novembro<o>, 3456<o>, 
                           Apt. 21 C<o>, Centro district<o>, postal code 04021-002<o>, located in the city of São Paulo/SP<o>, 
                           providing contact information: phone (11) 34134-0021.
             */
        }

        #endregion

        #region ▼ Remove

        [Test]
        public void Test16_RemoveFirst()
        {
            string firstMatch = TextBuilder.RemoveFirst(testText, "Marie Doe Towner ");
            Console.WriteLine(firstMatch);

            //Duration: 1ms, Memory: 6216 bytes

            /*RETURN: 
            A.2. PROMISING BUYER(S), married with 'John Doe Towner' , is Brazilian national, broker, 
                           married, registered under action and react law company, duly act registered 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021.
             */
        }

        [Test]
        public void Test16a_Remove()
        {
            string firstMatch = TextBuilder.Remove(testText, ",");
            Console.WriteLine(firstMatch);

            //Duration: 1ms, Memory: 6216 bytes

            /*RETURN: 
            A.2. PROMISING BUYER(S), married with 'John Doe Towner' , is Brazilian national, broker, 
                           married, registered under action and react law company, duly act registered 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021.
             */
        }

        #endregion

        #region ▼ Repalce

        [Test]
        public void Test17_ReplaceFirst()
        {
            string firstMatch = TextBuilder.ReplaceFirst(testText, "Marie Doe Towner", "Jene Doe Sanders");
            Console.WriteLine(firstMatch);

            //Duration: 1ms, Memory: 6216 bytes

            /*RETURN: 
            A.2. PROMISING BUYER(S), married with 'John Doe Towner' , Jene Doe Sanders is Brazilian national, broker, 
                           married, registered under action and react law company, duly act registered 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021.
             */
        }

        [Test]
        public void Test17a_Replace()
        {
            string firstMatch = TextBuilder.Replace(testText, ",", "<o>");
            Console.WriteLine(firstMatch);

            //Duration: 1ms, Memory: 6216 bytes

            /*RETURN: 
            A.2. PROMISING BUYER(S), married with 'John Doe Towner' , is Brazilian national, broker, 
                           married, registered under action and react law company, duly act registered 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021.
             */
        }

        #endregion

        #region ▼ Translate

        [Test]
        public void Test18_TranslateFirst()
        {
            string firstMatch = TextBuilder.TranslateFirst(testText, "Doe;married;,",
                                                                    "Silva;Divorced;<o>");
            Console.WriteLine(firstMatch);

            //Duration: 1ms, Memory: 6216 bytes

            /*RETURN: 
           A.2. PROMISING BUYER(S)<o> Divorced with 'John Silva Towner' , Marie Doe Towner is Brazilian national, broker, 
                           married, registered under action and react law company, duly act registered 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021.
             */
        }

        [Test]
        public void Test18a_Translate()
        {
            string firstMatch = TextBuilder.Translate(testText, "Doe;married;,", "Silva;Divorced;<o>");
            Console.WriteLine(firstMatch);

            //Duration: 1ms, Memory: 6216 bytes

            /*RETURN: 
            A.2. PROMISING BUYER(S)<o> Divorced with 'John Silva Towner' <o> Marie Silva Towner is Brazilian national<o> broker<o> 
                           Divorced<o> registered under action and react law company<o> duly act registered 
                           CPF number 675.019.610-18<o> RG number 23.300.225-3 SSP<o> residing at Rua XV de Novembro<o> 3456<o> 
                           Apt. 21 C<o> Centro district<o> postal code 04021-002<o> located in the city of São Paulo/SP<o> 
                           providing contact information: phone (11) 34134-0021.
             */
        }

        #endregion

        #region ▼ Contains

        [Test]
        public void Test19_Contains()
        {
            bool returnContains = TextBuilder.Contains(tinyText, "John ");
            Console.WriteLine(returnContains);

            /*RETURN: True*/
        }

        [Test]
        public void Test19a_Contains()
        {
            bool returnContains = TextBuilder.Contains(tinyText, "kkkkkkkbua");
            Console.WriteLine(returnContains);

            /*RETURN: False*/
        }

        [Test]
        public void Test19b_Contains()
        {
            bool returnContains = TextBuilder.Contains(tinyText, "Mar*ner");
            Console.WriteLine(returnContains);

            /*RETURN: True*/
        }

        #endregion

        #region ▼ Cont

        [Test]
        public void Test20_Cont()
        {
            int returnContains = TextBuilder.Cont(tinyText, "act");
            Console.WriteLine(returnContains);

            /*RETURN: 6 - reaction, reactor, react, action, act, contact*/
        }

        [Test]
        public void Test20a_Cont()
        {
            int returnContains = TextBuilder.Cont(tinyText, "r*act");
            Console.WriteLine(returnContains);

            /*RETURN: 4 - reaction, reactor, react, resented in this act*/
        }

        [Test]
        public void Test20b_Cont()
        {
            int returnContains = TextBuilder.Cont(tinyText, "r*act", TextOpt.MatchWholeWordOnly);
            Console.WriteLine(returnContains);

            /*RETURN: 4 - reaction, reactor, react*/
        }

        #endregion

        #region ▼ Append

        [Test]
        public void Test21_Append()
        {
            string firstMatch = TextBuilder.Append(testText, " additional text");
            Console.WriteLine(firstMatch);

            //Duration: 1ms, Memory: 6216 bytes

            /*RETURN: 
             A.2. PROMISING BUYER(S), married with 'John Doe Towner' , Marie Doe Towner the client is Brazilian national, broker, 
                           married, registered under action and react law company, duly act registered 
                           CPF number 675.019.610-18, RG number 23.300.225-3 SSP, residing at Rua XV de Novembro, 3456, 
                           Apt. 21 C, Centro district, postal code 04021-002, located in the city of São Paulo/SP, 
                           providing contact information: phone (11) 34134-0021.
             */
        }
        #endregion

        #endregion

        #region ▼ Snippets

        #region ▼ Matches

        [Test]
        public void Test30_Snippet()
        {
            StringAndPosition snippetMatch = TextBuilder.Snippet(html,"<div", "</div>");

            if (snippetMatch.Empty)
            { Console.WriteLine("Snippet not found!"); }
            else { Console.WriteLine(snippetMatch.Text); }

            //Duration: 2ms, Memory: 7240 bytes

            /*RETURN:
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
                                        divPopupVertical</div>
             */
        }

        [Test]
        public void Test30a_Snippet_with_identify()
        {
            StringAndPosition snippetMatch = TextBuilder.Snippet(html,"<div*id='divTemp'", "</div>");

            if (snippetMatch.Empty)
            { Console.WriteLine("Snippet not found!"); }
            else { Console.WriteLine(snippetMatch.Text); }

            //Duration: 2ms, Memory: 5568 bytes

            /*RETURN:
             <div id='divTemp' class='divPopupVertical popupColorBack_mid popupClosed' data-info='header'>
                                                    <input type='button' class='btnPopupClose popupColorBack_dark' value='x' onclick='subPopup_vertical_OpenClose()'/>
                                                    <span id='divUnitPopup_block_caption' data-info='label_bloco' class='lblTorre_label popupColorBack_mid'>TORRE:</span>
                                                    <div id='divUnitPopup_block' data-info='bloco' class='lblTorre_value popupColorBack_mid'>divTemp</div>
                                                    <span id='divUnitPopup_group_caption' data-info='label_grupo' class='lblAndar_label popupColorBack_mid'>ANDAR:</span>
                                                    <div id='divUnitPopup_group' data-info='quadra' class='lblAndar_value popupColorBack_mid'></div>
                                                    <span id='divUnitPopup_unit_caption' data-info='label_unidade' class='lblUnidade_label popupColorBack_mid'>APTO:</span>
                                                    <div id='divUnitPopup_unit' data-info='lote' class='lblUnidade_value popupColorBack_mid'></div>
                                                </div>
             */
        }

        #endregion

        #region ▼ Insert

        [Test]
        public void Test31_Insert()
        {
            string firstMatch = TextBuilder.InsertSnippet(tinyHtml, " <input id='btnElementInserted' type='button' value='OK' />\n\t  ", 346);
            Console.WriteLine(firstMatch);

            //Duration: 1ms, Memory: 6216 bytes

            /*RETURN: 
             <section id='popup'>
               <div id='divPopupVertical' class='divPopupVertical popupColorBack_light popupClosed'>teste
                <div id='divUnitPopup' class='divPopupVertical popupColorBack_mid popupOpened' data-unidade=''>                
                  <div id='divTemp' class='divPopupVertical popupColorBack_mid popupClosed' data-info='header'>
                  <input id='btnElementInserted' type='button' value='OK' />
	              <span id='divUnitPopup_block_caption' data-info='label_bloco' class='lblTorre_label popupColorBack_mid'>TORRE:</span>
                  <div id='divUnitPopup_block' data-info='bloco' class='lblTorre_value popupColorBack_mid'>divTemp</div>
                  <span id='divUnitPopup_group_caption' data-info='label_grupo' class='lblAndar_label popupColorBack_mid'>ANDAR:</span>
                  <div id='divUnitPopup_group' data-info='quadra' class='lblAndar_value popupColorBack_mid'></div>
                  <span id='divUnitPopup_unit_caption' data-info='label_unidade' class='lblUnidade_label popupColorBack_mid'>APTO:</span>
                  <div id='divUnitPopup_unit' data-info='lote' class='lblUnidade_value popupColorBack_mid'></div>
               </div>
	          </div>  
            </div>
           </section>
             */
        }

        [Test]
        public void Test31a_InsertBefore()
        {
            string firstMatch = TextBuilder.InsertSnippetBefore(tinyHtml, "<div*divUnitPopup_group",
                                                  "/div>", "   <input type='button' value='OK' />\n\t");
            Console.WriteLine(firstMatch);

            //Duration: 1ms, Memory: 6216 bytes

            /*RETURN: 
         <section id='popup'>
           <div id='divPopupVertical' class='divPopupVertical popupColorBack_light popupClosed'>teste
             <div id='divUnitPopup' class='divPopupVertical popupColorBack_mid popupOpened' data-unidade=''>                
               <div id='divTemp' class='divPopupVertical popupColorBack_mid popupClosed' data-info='header'>
                 <span id='divUnitPopup_block_caption' data-info='label_bloco' class='lblTorre_label popupColorBack_mid'>TORRE:</span>
                 <div id='divUnitPopup_block' data-info='bloco' class='lblTorre_value popupColorBack_mid'>divTemp</div>
                 <span id='divUnitPopup_group_caption' data-info='label_grupo' class='lblAndar_label popupColorBack_mid'>ANDAR:</span>
                 <input type='button' value='OK' />
	             <div id='divUnitPopup_group' data-info='quadra' class='lblAndar_value popupColorBack_mid'></div>
                 <span id='divUnitPopup_unit_caption' data-info='label_unidade' class='lblUnidade_label popupColorBack_mid'>APTO:</span>
                 <div id='divUnitPopup_unit' data-info='lote' class='lblUnidade_value popupColorBack_mid'></div>
               </div>
	         </div>  
           </div>
         </section>
             */
        }

        [Test]
        public void Test31b_InsertBefore_all_snippets_patterns()
        {
            string firstMatch = TextBuilder.InsertSnippetBefore(tinyHtml, "<span",
                                                  "/span>", "   <input type='button' value='OK' />\n\t");
            Console.WriteLine(firstMatch);

            //Duration: 1ms, Memory: 6216 bytes

            /*RETURN: 
        <section id='popup'>
          <div id='divPopupVertical' class='divPopupVertical popupColorBack_light popupClosed'>teste
           <div id='divUnitPopup' class='divPopupVertical popupColorBack_mid popupOpened' data-unidade=''>                
            <div id='divTemp' class='divPopupVertical popupColorBack_mid popupClosed' data-info='header'>
             <input type='button' value='OK' />
	         <span id='divUnitPopup_block_caption' data-info='label_bloco' class='lblTorre_label popupColorBack_mid'>TORRE:</span>
             <div id='divUnitPopup_block' data-info='bloco' class='lblTorre_value popupColorBack_mid'>divTemp</div>
             <input type='button' value='OK' />
	         <span id='divUnitPopup_group_caption' data-info='label_grupo' class='lblAndar_label popupColorBack_mid'>ANDAR:</span>
             <div id='divUnitPopup_group' data-info='quadra' class='lblAndar_value popupColorBack_mid'></div>
             <input type='button' value='OK' />
	         <span id='divUnitPopup_unit_caption' data-info='label_unidade' class='lblUnidade_label popupColorBack_mid'>APTO:</span>
             <div id='divUnitPopup_unit' data-info='lote' class='lblUnidade_value popupColorBack_mid'></div>
           </div>
	      </div>  
         </div>
        </section>
             */
        }

        [Test]
        public void Test31c_InsertAfter()
        {
            string firstMatch = TextBuilder.InsertSnippetAfter(tinyHtml, "<div*divUnitPopup_group",
                                             "/div>", "\n\t   <input type='button' value='OK' />");
            Console.WriteLine(firstMatch);

            //Duration: 1ms, Memory: 6216 bytes

            /*RETURN: 
             <section id='popup'>
              <div id='divPopupVertical' class='divPopupVertical popupColorBack_light popupClosed'>teste
               <div id='divUnitPopup' class='divPopupVertical popupColorBack_mid popupOpened' data-unidade=''>                
                <div id='divTemp' class='divPopupVertical popupColorBack_mid popupClosed' data-info='header'>
                <span id='divUnitPopup_block_caption' data-info='label_bloco' class='lblTorre_label popupColorBack_mid'>TORRE:</span>
                <div id='divUnitPopup_block' data-info='bloco' class='lblTorre_value popupColorBack_mid'>divTemp</div>
                <span id='divUnitPopup_group_caption' data-info='label_grupo' class='lblAndar_label popupColorBack_mid'>ANDAR:</span>
                <div id='divUnitPopup_group' data-info='quadra' class='lblAndar_value popupColorBack_mid'></div>
	            <input type='button' value='OK' />
                <span id='divUnitPopup_unit_caption' data-info='label_unidade' class='lblUnidade_label popupColorBack_mid'>APTO:</span>
                <div id='divUnitPopup_unit' data-info='lote' class='lblUnidade_value popupColorBack_mid'></div>
              </div>
	         </div>  
            </div>
           </section>
             */
        }

        #endregion

        #region ▼ Remove

        [Test]
        public void Test32_RemoveFirst()
        {
            string firstMatch = TextBuilder.RemoveSnippetFirst(tinyHtml, "<div*divUnitPopup_group", "/div>");
                        
            Console.WriteLine(firstMatch);

            //Duration: 1ms, Memory: 6216 bytes

            /*RETURN: 
            <section id='popup'>
             <div id='divPopupVertical' class='divPopupVertical popupColorBack_light popupClosed'>teste
              <div id='divUnitPopup' class='divPopupVertical popupColorBack_mid popupOpened' data-unidade=''>                
               <div id='divTemp' class='divPopupVertical popupColorBack_mid popupClosed' data-info='header'>
                <span id='divUnitPopup_block_caption' data-info='label_bloco' class='lblTorre_label popupColorBack_mid'>TORRE:</span>
                <div id='divUnitPopup_block' data-info='bloco' class='lblTorre_value popupColorBack_mid'>divTemp</div>
                <span id='divUnitPopup_group_caption' data-info='label_grupo' class='lblAndar_label popupColorBack_mid'>ANDAR:</span>
                
                <span id='divUnitPopup_unit_caption' data-info='label_unidade' class='lblUnidade_label popupColorBack_mid'>APTO:</span>
                <div id='divUnitPopup_unit' data-info='lote' class='lblUnidade_value popupColorBack_mid'></div>
               </div>
	          </div>  
             </div>
            </section>
             */
        }

        [Test]
        public void Test32a_Remove()
        {
            string firstMatch = TextBuilder.RemoveSnippet(tinyHtml, "<span", "/span>*\r\n");
            Console.WriteLine(firstMatch);

            //Duration: 1ms, Memory: 6216 bytes

            /*RETURN: 
            <section id='popup'>
             <div id='divPopupVertical' class='divPopupVertical popupColorBack_light popupClosed'>teste
              <div id='divUnitPopup' class='divPopupVertical popupColorBack_mid popupOpened' data-unidade=''>                
               <div id='divTemp' class='divPopupVertical popupColorBack_mid popupClosed' data-info='header'>
                <div id='divUnitPopup_block' data-info='bloco' class='lblTorre_value popupColorBack_mid'>divTemp</div>
                <div id='divUnitPopup_group' data-info='quadra' class='lblAndar_value popupColorBack_mid'></div>
                <div id='divUnitPopup_unit' data-info='lote' class='lblUnidade_value popupColorBack_mid'></div>
               </div>
	          </div>  
             </div>
            </section>
             */
        }

        #endregion

        #region ▼ Repalce

        [Test]
        public void Test33_ReplaceFirst()
        {
            string firstMatch = TextBuilder.ReplaceSnippetFirst(tinyHtml, 
                                                  "<div*divUnitPopup_group", "/div>", 
                                                       "<article id='Replaced_element' class='cssReplaces'></article>");
            Console.WriteLine(firstMatch);

            //Duration: 1ms, Memory: 6216 bytes

            /*RETURN: 
            <section id='popup'>
             <div id='divPopupVertical' class='divPopupVertical popupColorBack_light popupClosed'>teste
              <div id='divUnitPopup' class='divPopupVertical popupColorBack_mid popupOpened' data-unidade=''>                
               <div id='divTemp' class='divPopupVertical popupColorBack_mid popupClosed' data-info='header'>
                <span id='divUnitPopup_block_caption' data-info='label_bloco' class='lblTorre_label popupColorBack_mid'>TORRE:</span>
                <div id='divUnitPopup_block' data-info='bloco' class='lblTorre_value popupColorBack_mid'>divTemp</div>
                <span id='divUnitPopup_group_caption' data-info='label_grupo' class='lblAndar_label popupColorBack_mid'>ANDAR:</span>
                <article id='Replaced_element' class='cssReplaces'></article>
                <span id='divUnitPopup_unit_caption' data-info='label_unidade' class='lblUnidade_label popupColorBack_mid'>APTO:</span>
                <div id='divUnitPopup_unit' data-info='lote' class='lblUnidade_value popupColorBack_mid'></div>
               </div>
	          </div>  
             </div>
            </section>
             */
        }

        [Test]
        public void Test33a_Replace()
        {
            string firstMatch = TextBuilder.ReplaceSnippet(tinyHtml,
                                                  "<span", "/span>",
                                                       "<article id='Replaced_element' class='cssReplaces'></article>");
            Console.WriteLine(firstMatch);

            //Duration: 1ms, Memory: 6216 bytes

            /*RETURN: 
            <section id='popup'>
             <div id='divPopupVertical' class='divPopupVertical popupColorBack_light popupClosed'>teste
              <div id='divUnitPopup' class='divPopupVertical popupColorBack_mid popupOpened' data-unidade=''>                
               <div id='divTemp' class='divPopupVertical popupColorBack_mid popupClosed' data-info='header'>
                <article id='Replaced_element' class='cssReplaces'></article>
                <div id='divUnitPopup_block' data-info='bloco' class='lblTorre_value popupColorBack_mid'>divTemp</div>
                <article id='Replaced_element' class='cssReplaces'></article>
                <div id='divUnitPopup_group' data-info='quadra' class='lblAndar_value popupColorBack_mid'></div>
                <article id='Replaced_element' class='cssReplaces'></article>
                <div id='divUnitPopup_unit' data-info='lote' class='lblUnidade_value popupColorBack_mid'></div>
               </div>
	          </div>  
             </div>
            </section>
             */
        }

        #endregion

        #region ▼ Contains

        [Test]
        public void Test34_Contains()
        {
            bool returnContains = TextBuilder.ContainsSnippet(tinyHtml, "<div*divUnitPopup_group", "/div>");
            Console.WriteLine(returnContains);

            /*RETURN: True*/
        }

        [Test]
        public void Test34a_Contains()
        {
            bool returnContains = TextBuilder.ContainsSnippet(tinyHtml, "<article", "/article>");
            Console.WriteLine(returnContains);

            /*RETURN: False*/
        }

        #endregion

        #region ▼ Cont

        [Test]
        public void Test35_Cont()
        {
            int returnContains = TextBuilder.ContSnippets(tinyHtml, "<div*divUnitPopup_group", "/div>");
            Console.WriteLine(returnContains);

            /*RETURN: 1 */
        }

        [Test]
        public void Test35a_Cont()
        {
            int returnContains = TextBuilder.ContSnippets(tinyHtml, "<span", "/span>");
            Console.WriteLine(returnContains);

            /*RETURN: 3 */
        }

        #endregion

        #endregion
    }
}
