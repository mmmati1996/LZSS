using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using System.Windows;

namespace LZSS
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string bin_filename = "Decompressed.lzss";
        string compressed;
        private System.Timers.Timer aTimer;

        public MainWindow()
        {
            InitializeComponent();
            
            //Console.WriteLine(compressed);
            //Console.WriteLine(Decompress_LZSS(compressed, 4, 4));
        }
        private void SetTimer()
        {
            // Create a timer with a two second interval.
            aTimer = new System.Timers.Timer(10);

            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }
        private void OnTimedEvent(object source, ElapsedEventArgs e)
        {
            textBlock_Time.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new TBXTextChanger(WriteToTextBoxTimer), (e.SignalTime).ToString());
        }
        private string Compress_LZSS(string fulltext_input, int dictionary_size, int input_buffer_size)
        {
            try
            {
                //SetTimer();
                DateTime date1 = DateTime.Now;
                int fulltextsize = fulltext_input.Length;
                int amount3 = 0;
                int amount2 = 0;
                string dictionary = string.Empty;
                string input_buffer = fulltext_input.Substring(0, input_buffer_size);
                fulltext_input = fulltext_input.Substring(input_buffer_size);
                //output format: flag + offset + length [[ 1 bit + dictionary_size + input_buffer_size ]]
                int output_size = 1 + (int)Math.Log(dictionary_size, 2) + (int)Math.Log(input_buffer_size, 2);

                string output = string.Empty;


                while (input_buffer.Length > 0)
                {
                    int longestword_offset = 0;
                    int longestword_length = 0;

                    for (int i = 1; i <= input_buffer.Length; i++)
                    {
                        string word = input_buffer.Substring(0, i);
                        if (dictionary.Contains(word))
                        {
                            if (longestword_length < i)
                            {
                                longestword_length = i;
                                longestword_offset = dictionary.IndexOf(word);
                            }
                            else
                            {
                                break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (output_size < 8 * longestword_length)
                    {
                        output += "0" + Convert.ToString(longestword_offset, 2).PadLeft((int)Math.Log(dictionary_size, 2), '0') + Convert.ToString(longestword_length - 1, 2).PadLeft((int)Math.Log(input_buffer_size, 2), '0');
                        dictionary += input_buffer.Substring(0, longestword_length);
                        if (dictionary.Length > dictionary_size)
                            dictionary = dictionary.Remove(0, longestword_length);
                        input_buffer = input_buffer.Remove(0, longestword_length);
                        if (longestword_length > fulltext_input.Length)
                            input_buffer += fulltext_input;
                        else
                            input_buffer += fulltext_input.Substring(0, longestword_length);
                        if (longestword_length < fulltext_input.Length)
                            fulltext_input = fulltext_input.Substring(longestword_length);
                        else
                            fulltext_input = string.Empty;
                        amount3++;
                    }
                    else
                    {
                        output += "1" + Convert.ToString(Convert.ToInt32(input_buffer[0]), 2).PadLeft(8, '0');
                        dictionary += input_buffer.Substring(0, 1);
                        if (dictionary.Length > dictionary_size)
                            dictionary = dictionary.Remove(0, 1);
                        input_buffer = input_buffer.Remove(0, 1);
                        if (fulltext_input.Length > 0)
                        {
                            input_buffer += fulltext_input.Substring(0, 1);
                            fulltext_input = fulltext_input.Substring(1);
                        }
                        amount2++;
                    }
                }
                //aTimer.Stop();
                //aTimer.Dispose();
                DateTime date2 = DateTime.Now;
                TimeSpan span = date2 - date1;
                string time_formating = "" + span.Minutes + ":" + span.Seconds + ":" + span.Milliseconds;
                textBlock_Time.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new TBXTextChanger(WriteToTextBoxTimer), time_formating);
                textBlock_Info.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new TBXTextChanger(this.WriteToTextBox), "File is compressed, now you can save it");
                return output;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                MessageBox.Show("There was an error during this operation.");
                textBlock_Info.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new TBXTextChanger(this.WriteToTextBox), "An error has ocured during Compression please try again");
                return null;
            }
        }

        private string Decompress_LZSS(string fulltext_input, int offset_size, int length_size)
        {
            try
            {
                DateTime date1 = DateTime.Now;
                string output = string.Empty;
                int offset_size_bit = (int)Math.Log(offset_size, 2);
                int length_size_bit = (int)Math.Log(length_size, 2);


                while (fulltext_input.Length > 0)
                {

                    if (fulltext_input[0] == '1')
                    {
                        output += Convert.ToChar(Convert.ToInt32(fulltext_input.Substring(1, 8), 2));
                        fulltext_input = fulltext_input.Substring(9);
                    }
                    else
                    {
                        int offset = Convert.ToInt32(fulltext_input.Substring(1, offset_size_bit), 2);
                        int length = Convert.ToInt32(fulltext_input.Substring(1 + offset_size_bit, length_size_bit), 2);
                        if (output.Length <= length_size)
                            output += output.Substring(offset, length + 1);
                        else
                        {
                            string lastchars = output.Substring(output.Length - length_size, length_size);
                            output += lastchars.Substring(offset, length + 1);
                        }
                        fulltext_input = fulltext_input.Substring(1 + length_size_bit + offset_size_bit);
                    }
                }
                DateTime date2 = DateTime.Now;
                TimeSpan span = date2 - date1;
                string time_formating = "" + span.Minutes + ":" + span.Seconds + ":" + span.Milliseconds;
                textBlock_Time.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new TBXTextChanger(WriteToTextBoxTimer), time_formating);
                textBlock_Info.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new TBXTextChanger(this.WriteToTextBox), "Decompression was successful.");
                return output;
            }
            catch (ArgumentOutOfRangeException ex)
            {
                MessageBox.Show("There was an error during this operation.\nFile might be corrupted.");
                textBlock_Info.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new TBXTextChanger(this.WriteToTextBox), "An error has ocured during decompression please try again");
                return null;
            }
        }

        private void Button_OpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Text files (*.txt)|*.txt|Binary files (*.dat)|*.dat",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments)
            };
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var filePath = openFileDialog.FileName;
                    var fileStream = openFileDialog.OpenFile();
                    string ext = System.IO.Path.GetExtension(openFileDialog.FileName);
                    if (string.Equals(ext, ".txt"))
                    { 
                        using (StreamReader reader = new StreamReader(fileStream))
                        {
                            textBox_Input.Text = reader.ReadToEnd();
                        }
                        textBlock_Info.Text = "File is loaded, now you can compress it";
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new ControlChanger(Button_Compress_Switch), true);
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new ControlChanger(Button_Decompress_Switch), false);
                    }
                    else if (string.Equals(ext, ".dat"))
                    {
                        textBlock_Info.Text = "Loading the file. Please wait...";
                        Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new ControlChanger(AllControlsSwitch), false);
                        new Thread(() =>
                        {
                            try
                            {
                                using (BinaryReader reader = new BinaryReader(fileStream))
                                {
                                    byte[] fileData = reader.ReadBytes((int)fileStream.Length);
                                    StringBuilder sb = new StringBuilder();
                                    string strBin = string.Empty;
                                    byte btindx = 0;
                                    string strAllbin = string.Empty;

                                    for (int i = 0; i < fileData.Length; i++)
                                    {
                                        btindx = fileData[i];

                                        strBin = Convert.ToString(btindx, 2);
                                        strBin = strBin.PadLeft(8, '0');

                                        strAllbin += strBin;
                                    }
                                    compressed = strAllbin.TrimStart('0');
                                    textBlock_Info.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new TBXTextChanger(this.WriteToTextBox), "File is loaded, now you can decompress it");
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new ControlChanger(AllControlsSwitch), true);
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new ControlChanger(Button_Compress_Switch), false);
                                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new ControlChanger(Button_Decompress_Switch), true);
                                }
                            }catch (Exception ex) { MessageBox.Show("Unable to load the file.\nFile might be corrupted."); }
                        }).Start();
                    }       
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error");
                }
            }
        }

        private void Button_SaveFile_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
            {
                Filter = "Binary files (*.dat)|*.dat",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
                Title = "Save your compressed file"

            };

            if(saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (Stream stream = saveFileDialog.OpenFile())
                    {
                        using (BinaryWriter bw = new BinaryWriter(stream))
                        {
                            /*byte[] output = new byte[(compressed.Length / 8) + 1];

                            for (int i = 0; i < output.Length; i++)
                            {
                                for (int b = 0; b <= 7; b++)
                                {
                                    output[i] |= (byte)((compressed[i * 8 + b] == '1' ? 1 : 0) << (7 - b));
                                    Console.WriteLine(compressed[i * 8 + b]);
                                }
                                Console.WriteLine(output[i]);
                                bw.Write(output[i]);
                            }*/
                            var bitsToPad = 8 - compressed.Length % 8;

                            if (bitsToPad != 8)
                            {
                                var neededLength = bitsToPad + compressed.Length;
                                compressed = compressed.PadLeft(neededLength, '0');
                            }

                            int size = compressed.Length / 8;
                            byte[] arr = new byte[size];

                            for (int a = 0; a < size; a++)
                            {
                                arr[a] = Convert.ToByte(compressed.Substring(a * 8, 8), 2);
                                bw.Write(arr[a]);
                            }
                        }
                    }
                    textBlock_Info.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new TBXTextChanger(this.WriteToTextBox), "The file has been saved");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error");
                }
            }
        }
        bool IsPowerOfTwo(long x)
        {
            return (x != 0) && ((x & (x - 1)) == 0);
        }
        private delegate void TBXTextChanger(string text);
        private void WriteToTextBox(string text)
        {
            if (this.textBlock_Info.Dispatcher.CheckAccess())
            {
                this.textBlock_Info.Text = text;
            }
        }
        private void WriteToTextBoxTimer(string text)
        {
            if (this.textBlock_Time.Dispatcher.CheckAccess())
            {
                this.textBlock_Time.Text = text;
            }
        }

        private void WriteToTextBoxOutput(string text)
        {
            if (this.textBox_Output.Dispatcher.CheckAccess())
            {
                this.textBox_Output.Text = text;
            }
        }

        private delegate void ControlChanger(bool enable);
        private void Button_Compress_Switch(bool enable)
        {
            if (textBox_Output.Dispatcher.CheckAccess())
            {
                if (enable == true)
                {
                    button_Compress.IsEnabled = true;
                }
                else
                {
                    button_Compress.IsEnabled = false;
                }
            }
        }
        private void Button_Decompress_Switch(bool enable)
        {
            if (textBox_Output.Dispatcher.CheckAccess())
            {
                if (enable == true)
                {
                    button_Decompress.IsEnabled = true;
                }
                else
                {
                    button_Decompress.IsEnabled = false;
                }
            }
        }

        private void AllControlsSwitch(bool enable)
        {
            if (textBox_Output.Dispatcher.CheckAccess())
            {
                if (enable == true)
                {
                    button_Compress.IsEnabled = true;
                    button_Decompress.IsEnabled = true;
                    button_OpenFile.IsEnabled = true;
                    button_SaveFile.IsEnabled = true;
                    textBox_Buffer.IsEnabled = true;
                    textBox_Dictionary.IsEnabled = true;
                    textBox_Input.IsEnabled = true;
                    textBox_Output.IsEnabled = true;
                }
                else
                {
                    button_Compress.IsEnabled = false;
                    button_Decompress.IsEnabled = false;
                    button_OpenFile.IsEnabled = false;
                    button_SaveFile.IsEnabled = false;
                    textBox_Buffer.IsEnabled = false;
                    textBox_Dictionary.IsEnabled = false;
                    textBox_Input.IsEnabled = false;
                    textBox_Output.IsEnabled = false;
                }
            }
        }
        private void Button_Compress_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (IsPowerOfTwo(Convert.ToInt64(textBox_Buffer.Text)) == false || IsPowerOfTwo(Convert.ToInt64(textBox_Dictionary.Text)) == false) throw new InvalidDataException();
                textBlock_Info.Text = "File is being compressed. Please wait...";
                var input = textBox_Input.Text;
                var dictionarty = textBox_Dictionary.Text;
                var buffer = textBox_Buffer.Text;
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new ControlChanger(AllControlsSwitch), false);
                new Thread(() =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    compressed = Compress_LZSS(input, Convert.ToInt32(dictionarty), Convert.ToInt32(buffer));
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new ControlChanger(AllControlsSwitch), true);
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new ControlChanger(Button_Decompress_Switch), false);

                }).Start();
                
            }
            catch (InvalidDataException ex) { MessageBox.Show("Dictionary or Buffer is not power of two"); } 
            catch (Exception ex) { MessageBox.Show("An error has occured please try again"); }
        }

        private void Button_Decompress_Click(object sender, RoutedEventArgs e)
        {
            try
            {

                var dictionarty = textBox_Dictionary.Text;
                var buffer = textBox_Buffer.Text;
                Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new ControlChanger(AllControlsSwitch), false);
                new Thread(() =>
                {
                    textBox_Output.Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new TBXTextChanger(this.WriteToTextBoxOutput), Decompress_LZSS(compressed, Convert.ToInt32(dictionarty), Convert.ToInt32(buffer)));
                    
                    Dispatcher.Invoke(System.Windows.Threading.DispatcherPriority.Normal, new ControlChanger(AllControlsSwitch), true);

                }).Start();
            }
            catch (ArgumentOutOfRangeException ex) { MessageBox.Show("There was an error during this operation.\nFile might be corrupted."); }
        }

        private void Button_OpenFile_Statistics_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "pliki tekstowe .txt|*.txt";
            if (openFileDialog.ShowDialog() == true)
            {
                textBox_Input_Statistics.Text = File.ReadAllText(openFileDialog.FileName);

                string fulltext = textBox_Input_Statistics.Text;


                int maxchar = 0;

                foreach (char c in fulltext)
                {
                    if (c > maxchar) maxchar = c;
                }


                char[] chars = new char[maxchar + 1];

                foreach (char c in fulltext)
                {
                    chars[c]++;
                }

                var dictionary = new Dictionary<int, int>();

                for (int i = 0; i <= maxchar; i++)
                {
                    if (chars[i] > 0)
                        dictionary.Add(i, chars[i]);
                }

                var items = from pair in dictionary
                            orderby pair.Value descending
                            select pair;

                double entropia = 0;
                double charscount = 0;

                foreach (KeyValuePair<int, int> pair in items)
                {
                    charscount += pair.Value;
                }
                textBox_Output_Statistics.Text = "Długość tekstu " + fulltext.Length + " znaków";
                textBox_Output_Statistics.Text += Environment.NewLine + "Kod najwyższego znaku " + maxchar;
                if (maxchar > 255) textBox_Output_Statistics.Text += " kodowanie UTF-8";

                foreach (KeyValuePair<int, int> pair in items)
                {
                    if (pair.Key == 10)
                        textBox_Output_Statistics.Text += Environment.NewLine + "[" + pair.Key + "] '" + "NL" +
                                       "' wystąpień: " + pair.Value;
                    else if (pair.Key == 13)
                        textBox_Output_Statistics.Text += Environment.NewLine + "[" + pair.Key + "] '" + "CR" +
                                           "' wystąpień: " + pair.Value;

                    else if (pair.Key == 32)
                        textBox_Output_Statistics.Text += Environment.NewLine + "[" + pair.Key + "] '" + "SPACE" +
                                           "' wystąpień: " + pair.Value;
                    else
                        textBox_Output_Statistics.Text += Environment.NewLine + "[" + pair.Key + "] '" + Convert.ToChar(pair.Key) +
                                       "' wystąpień: " + pair.Value;

                    textBox_Output_Statistics.Text += " procentowo " + 100 * (float)pair.Value / charscount + "%";
                    entropia -= ((double)pair.Value / charscount) * Math.Log((double)pair.Value / charscount, 2);
                }

                textBox_Output_Statistics.Text = "Entropia (P) " + entropia + " bitów" + Environment.NewLine + textBox_Output_Statistics.Text;
            }
        }

        private void Button_SaveFile_Statistics_Click(object sender, RoutedEventArgs e)
        {

        }

        private static readonly Regex regex = new Regex("[^0-9]"); //regex that matches disallowed text
        private static bool IsTextAllowed(string text)
        {
            return !regex.IsMatch(text);
        }
        private void TextBox_Dictionary_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            e.Handled = !IsTextAllowed(e.Text);
        }
        private void TextBoxPasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(string)))
            {
                string text = (string)e.DataObject.GetData(typeof(string));
                if (!IsTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }
    }
}
