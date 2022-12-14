using System;
using System.Diagnostics;
using System.IO.Compression;
using System.Security.Policy;
using Microsoft.VisualBasic;

namespace Diffusion.Updater
{
    public partial class Form1 : Form
    {
        private bool _finished = false;
        private Release _selectedRelease;
        private bool _updateSuccessful;
        private UpdateChecker _updateChecker;

        private string _targetPath;
        private string _exePath;
        public Form1()
        {
            InitializeComponent();
        }

        private async void Form1_Load(object sender, EventArgs e)
        {

            string[] args = Environment.GetCommandLineArgs();

            if (args.Length < 2)
            {
                MessageBox.Show("You must specify the target path as an argument", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Application.Exit();
            }

            _targetPath = args[1];
            _exePath = Path.Join(_targetPath, "Diffusion.Toolkit.exe");

            if (!Directory.Exists(_targetPath) || !File.Exists(_exePath))
            {
                MessageBox.Show("Diffusion assembly was not found at the specified target", "Error", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                Application.Exit();
            }

            try
            {
                buttonOK.Enabled = false;
                _finished = true;
                buttonCancel.Text = "Close";

                _updateChecker = new UpdateChecker();

                if (await _updateChecker.CheckForUpdate(_targetPath))
                {
                    _selectedRelease = _updateChecker.LatestRelease;

                    textBoxNotes.Text = $"A new version is available. Do you want to update?\r\n\r\n{_selectedRelease.name}\r\n{new string('=', _selectedRelease.name.Length)}\r\n\r\n{_selectedRelease.body}\r\n\r\n";
                    buttonOK.Enabled = true;
                }
                else
                {
                    textBoxNotes.Text = $"Your version is up to date";
                    _finished = true;
                    buttonCancel.Text = "Close";
                }

            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Diffusion Updater", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
        }

        private void Clear()
        {
            textBoxNotes.Text = "";
        }

        private void Log(string text)
        {
            textBoxNotes.Text = textBoxNotes.Text + text + "\r\n";
            textBoxNotes.Select(textBoxNotes.Text.Length - 1, 1);
        }


        private async Task TryUpdate(Asset asset, CancellationToken token)
        {
            Clear();
            _finished = false;
            buttonCancel.Text = "Cancel";

            var url = asset.browser_download_url;

            Log($"Downloading {url}...");

            var stream = await _updateChecker.Client.DownloadAsync(url, token);

            var tempFile = Path.GetTempFileName();

            try
            {
                var buffer = new byte[4096];

                progressBar1.Maximum = asset.size;

                progressBar1.Value = 0;

                int counter = 0;

                using (var tempfs = new FileStream(tempFile, FileMode.Create, FileAccess.Write))
                {
                    var read = await stream.ReadAsync(buffer, 0, 4096, token);

                    while (read != 0)
                    {
                        progressBar1.Value += read;
                        //if (counter % 31 == 0)
                        //{
                        //    Log($"Downloading {((float)progressBar1.Value / progressBar1.Maximum) * 100:##0%}...");
                        //}
                        await tempfs.WriteAsync(buffer, 0, read, token);
                        read = await stream.ReadAsync(buffer, 0, 4096, token);
                    }


                    await tempfs.FlushAsync(token);
                }

                Log($"Extracting...");

                using (var zip = ZipFile.Open(tempFile, ZipArchiveMode.Read))
                {
                    zip.ExtractToDirectory(_targetPath, true);
                }

                Log($"Done!");
            }
            finally
            {
                File.Delete(tempFile);
            }

        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            if (_finished)
            {
                Close();
                if (_updateSuccessful)
                {
                    Process.Start(_exePath);
                }
            }
            else
            {
                _updateChecker.Cancel();
            }
        }

        private void TryKillProcess()
        {
            string targetProcessPath = Path.Join(_exePath);
            string targetProcessName = "Diffusion.Toolkit";

            Process[] runningProcesses = Process.GetProcesses();
            foreach (Process process in runningProcesses)
            {
                if (process.ProcessName == targetProcessName &&
                    process.MainModule != null &&
                    string.Compare(process.MainModule.FileName, targetProcessPath, StringComparison.InvariantCultureIgnoreCase) == 0)
                {
                    process.Kill();
                }
            }
        }

        private async void buttonOK_Click(object sender, EventArgs e)
        {
            try
            {
                buttonOK.Enabled = false;

                TryKillProcess();

                await TryUpdate(_selectedRelease.assets[0], _updateChecker.CancellationToken);

                if (_updateChecker.CancellationToken.IsCancellationRequested)
                {
                    MessageBox.Show("The update was cancelled!", "Diffusion Updater", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                else
                {
                    _updateSuccessful = true;
                    MessageBox.Show("Update complete!", "Diffusion Updater", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Diffusion Updater", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
            }
            finally
            {
                _finished = true;
            }

            buttonCancel.Text = "Close";
        }
    }
}