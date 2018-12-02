using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace LZSS
{
    /// <summary>
    /// Logika interakcji dla klasy MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string bin_filename = "Decompressed.lzss";

        public MainWindow()
        {
            InitializeComponent();
            string compressed = Compress_LZSS("aabbcababbASDFawefeWfwefWEFWEfasdfasDFAWEFasdFASDgaeGSDFgfsdgESGDGdfgSDGSGSdcgSDGSfsdgsDGSDRgdsfgSSDGSDFSGGSDGdsfgsdcgsRSGDGDGDSFSGDGSDGSDFSDFSGDFSGDF", 4, 4);
            Console.WriteLine(compressed);
            Console.WriteLine(Decompress_LZSS(compressed, 4, 4));
        }

        private string Compress_LZSS(string fulltext_input, int dictionary_size, int input_buffer_size)
        {
            int fulltextsize = fulltext_input.Length;
            int amount3 = 0;
            int amount2 = 0;
            string dictionary = string.Empty;
            string input_buffer = fulltext_input.Substring(0,input_buffer_size);
            fulltext_input = fulltext_input.Substring(input_buffer_size);
            //output format: flag + offset + length [[ 1 bit + dictionary_size + input_buffer_size ]]
            int output_size = 1 + (int)Math.Log(dictionary_size,2) + (int)Math.Log(input_buffer_size,2);

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
                    output += "0" + Convert.ToString(longestword_offset, 2).PadLeft((int)Math.Log(dictionary_size, 2), '0') + Convert.ToString(longestword_length-1, 2).PadLeft((int)Math.Log(input_buffer_size, 2), '0');
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
                    output += "1" + Convert.ToString(Convert.ToInt32(input_buffer[0]),2).PadLeft(8, '0');
                    dictionary += input_buffer.Substring(0, 1);
                    if (dictionary.Length > dictionary_size)
                        dictionary = dictionary.Remove(0, 1);
                    input_buffer = input_buffer.Remove(0, 1);
                    if(fulltext_input.Length > 0)
                    {
                        input_buffer += fulltext_input.Substring(0, 1);
                        fulltext_input = fulltext_input.Substring(1);
                    }
                    amount2++;
                }
            }
            return output;
        }

        private string Decompress_LZSS(string fulltext_input, int offset_size, int length_size)
        {
            string output = string.Empty;
            int offset_size_bit = (int)Math.Log(offset_size, 2);
            int length_size_bit = (int)Math.Log(length_size, 2);

            while(fulltext_input.Length > 0)
            {
                if(fulltext_input[0] == '1')
                {
                    output += Convert.ToChar(Convert.ToInt32(fulltext_input.Substring(1, 8), 2));
                    fulltext_input = fulltext_input.Substring(9);
                }
                else
                {
                    int offset = Convert.ToInt32(fulltext_input.Substring(1, offset_size_bit), 2);
                    int length = Convert.ToInt32(fulltext_input.Substring(1+offset_size_bit, length_size_bit), 2);
                    if (output.Length <= length_size)
                        output += output.Substring(offset, length+1);
                    else
                    {
                        string lastchars = output.Substring(output.Length - length_size, length_size);
                        output += lastchars.Substring(offset, length+1);
                    }
                    fulltext_input = fulltext_input.Substring(1 + length_size_bit + offset_size_bit);
                }
            }

            return output;
        }
    }
}
