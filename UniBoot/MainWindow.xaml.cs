using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace UniBoot
{
    public partial class MainWindow : Window
    {
        string GameDir = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), ".FroxieLaunchers");
        string localvers;

        WebClient webClient = new WebClient();
        public MainWindow()
        {

            InitializeComponent();

            if (File.Exists(GameDir+"\\UniLauncher.txt"))
            {
                localvers = File.ReadAllText(GameDir + "\\UniLauncher.txt");
                string onlinevers = webClient.DownloadString("http://84.235.226.133/unilauncher/unilaunchervers.txt");
                if (onlinevers!=localvers)
                {
                    File.Delete(GameDir + "\\UniLauncher.exe");
                    File.Delete(GameDir + "\\UniLauncher.txt");
                    Update();
                }else{
                    Start();
                }
            }
            else
            { 
                Directory.CreateDirectory(GameDir);
                Update();
            }

        }

        
        async Task Update() {
            await DownloadFileWithHttpClientAsync("http://84.235.226.133/unilauncher/UniLauncher.exe", GameDir+"\\UniLauncher.exe");
            await Start();
            webClient.DownloadFile("http://84.235.226.133/unilauncher/unilaunchervers.txt", GameDir + "\\UniLauncher.txt");

        }
         async Task Start() {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "UniLauncher.exe",
                WorkingDirectory = GameDir,
                UseShellExecute = true
            };

            using (var process = Process.Start(startInfo))
            {
                if (process != null)
                {
                    Close();
                }
            }
        }
        public async Task DownloadFileWithHttpClientAsync(string url, string destinationPath)
        {
            try
            {
                progressBar.Visibility = Visibility.Visible;
                progressBar.Value = 0;
                Status.Text = "Starting download...";

                using (var httpClient = new System.Net.Http.HttpClient())
                {
                    using (var response = await httpClient.GetAsync(url, System.Net.Http.HttpCompletionOption.ResponseHeadersRead))
                    {
                        response.EnsureSuccessStatusCode();

                        var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                        var canReportProgress = totalBytes != -1;

                        using (var contentStream = await response.Content.ReadAsStreamAsync())
                        using (var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                        {
                            var buffer = new byte[8192];
                            long totalRead = 0;
                            int bytesRead;

                            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                            {
                                await fileStream.WriteAsync(buffer, 0, bytesRead);
                                totalRead += bytesRead;

                                if (canReportProgress)
                                {
                                    var progress = (double)totalRead / totalBytes * 100;
                                    progressBar.Value = progress;
                                    Status.Text = $"Downloading... {progress:F1}%";
                                }
                            }
                        }
                    }
                }

                Status.Text = "Download complete!";
                progressBar.Value = 100;
            }
            catch (Exception ex)
            {
                Status.Text = "Download failed";
                MessageBox.Show($"Error: {ex.Message}", "Download Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                await Task.Delay(2000);
                progressBar.Visibility = Visibility.Hidden;
            }
        }
    }
}