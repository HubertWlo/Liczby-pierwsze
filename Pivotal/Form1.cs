using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Xml.Serialization;
using OfficeOpenXml;
using PrimeFinder;

namespace Pivotal
{
    public partial class Form1 : Form
    {
        private Timer timer;
        private List<Prime> primeResults;
        private string xmlFilePath = "PrimeNumbers.xml";
        private int counter = 1;
        public bool stopAutoSave = false;
        private Prime prime;
        public Form1()
        {
            primeResults = new List<Prime>();
            InitializeComponent();
            InitializeTimer();
            InitializePrime();
        }
        private void InitializeTimer()
        {
            timer = new Timer();
            timer.Tick += Timer_Tick;
        }
        private void InitializePrime()
        {
            prime = new Prime();
            prime.PrimeCalculated += Prime_PrimeCalculated;
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        public void Timer_Tick(object sender, EventArgs e)
        {
            if (!stopAutoSave)
            {
                int currentPrime;
                if (int.TryParse(label1.Text, out currentPrime))
                {
                    prime.Calculate(currentPrime);
                }
            }
            if (stopAutoSave)
            {
                timer.Stop();
                SaveXmlFile();
            }
        }
        public void button1_Click(object sender, EventArgs e)
        {
            timer.Interval = 60000; //!-----------
            stopAutoSave = false;
            timer.Start();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            stopAutoSave = true;
            prime.StopCalculating();
        }
        private void SaveXmlFile()
        {
            List<Prime> existingResults = new List<Prime>();
            if (File.Exists(xmlFilePath))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<Prime>));
                using (FileStream fs = new FileStream(xmlFilePath, FileMode.Open))
                {
                    existingResults = (List<Prime>)serializer.Deserialize(fs);
                }
            }
            existingResults.AddRange(primeResults);

            XmlSerializer xmlSerializer = new XmlSerializer(typeof(List<Prime>));
            using (FileStream fs = new FileStream(xmlFilePath, FileMode.Create))
            {
                xmlSerializer.Serialize(fs, existingResults);
            }
            MessageBox.Show("Plik XML zapisany pomyślnie.", "Informacja", MessageBoxButtons.OK, MessageBoxIcon.Information);
            primeResults.Clear();
        }
        private void Prime_PrimeCalculated(int primeNumber, string elapsedTimeCycle, int cycle, string elapsedTime)
        {
            if (primeNumber == -1)
            {
                MessageBox.Show("Nie udało się znaleźć liczby.", "Informacja", MessageBoxButtons.OK, MessageBoxIcon.Information);
                stopAutoSave = true;
                prime.StopCalculating();
            }
            label1.Text = primeNumber.ToString();
            label2.Text = "Wartości zapamiętane w trakcie ostatniego cyklu:";
            primeResults.Add(new Prime { PrimeNumber = primeNumber, ElapsedTimeCycle = elapsedTimeCycle + "m", Cycle = cycle , ElapsedTime = elapsedTime + "ms"});
        }
    }
}
namespace PrimeFinder
{
    public class Prime
    {
        public int PrimeNumber;
        public string ElapsedTimeCycle;
        public string ElapsedTime;
        public int Cycle = 0;
        private bool stopCalculating = false;
        public event Action<int, string, int, string> PrimeCalculated;
        public void StopCalculating()
        {
            stopCalculating = true;
        }
        public void Calculate(int prime)
        {
            stopCalculating = false;
            Stopwatch stopwatch = new Stopwatch();
            Stopwatch stopwatchCycle = new Stopwatch();
            stopwatchCycle.Start();
            int nextPrime = FindPrime(prime, stopwatchCycle, stopwatch);
            stopwatchCycle.Stop();
            PrimeCalculated?.Invoke(nextPrime, stopwatchCycle.Elapsed.TotalMinutes.ToString("0"), Cycle, stopwatch.Elapsed.TotalMilliseconds.ToString("0.00000"));
        }
        public int FindPrime(int last, Stopwatch stopwatchCycle, Stopwatch stopwatch)
        {
            bool done = false;
            stopwatchCycle.Start();
            int nextNumber = last + 1;
            int number = nextNumber;
            while (stopwatchCycle.Elapsed.TotalMinutes < 2 && !stopCalculating) //!!!!
            {
                stopwatch.Start();
                if (IsPrime(nextNumber))
                {
                    number = nextNumber;
                    done = true;
                    stopwatch.Reset();

                }
                nextNumber++;
            }
            this.Cycle++;
            stopwatchCycle.Stop();
            if (done)
            {
                stopwatch.Stop();
                return number;
            }
            else
            {
                return -1;
            }
        }
        public bool IsPrime(int n)
        {
            if (n <= 1) return false;
            if (n <= 3) return true;
            if (n % 2 == 0 || n % 3 == 0) return false;
            int i = 5;
            while (i * i <= n)
            {
                if (n % i == 0 || n % (i + 2) == 0) return false;
                i += 6;
            }
            return true;
        }
    }
}
