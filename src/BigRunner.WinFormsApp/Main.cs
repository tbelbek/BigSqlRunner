namespace BigRunner.WinFormsApp
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows.Forms;

    /// <summary>
    /// This indicates that it is a start point of the program
    /// </summary>
    public partial class Main : Form
    {
        /// <summary>
        /// Initialize BigRunner.WinFormsApp.Main() constructor
        /// </summary>
        public Main()
        {
            /**********************************************
				Initialize these necessary components
			**********************************************/
            InitializeComponent();

            /**********************************************
				Initialize some necessary configurations of
				worker
			**********************************************/
            bwRunBigSqlScript.WorkerSupportsCancellation = true;
        }

        #region Events
        /// <summary>
        /// This indicates that this event occurred when user clicks these buttons which have the value called "..."
        /// </summary>
        /// <param name="sender">sender</param>
        /// <param name="e">e</param>
        private void btnChooseFile_Click(object sender, EventArgs e)
        {
            if (!(sender is Button button)) return;

            switch (button.Name)
            {
                /**********************************************
					This case will be adopted for the first 
					"Choose File"
				**********************************************/
                case "btnChooseFile":
                    ofdDialog1.ShowDialog();
                    break;

                /**********************************************
					This case will be adopted for the second 
					"Choose File"
				**********************************************/
                case "btnChooseFile2":
                    ofdDialog2.ShowDialog();
                    break;
            }
        }

        /// <summary>
        /// This event is raised up when the users choose file in Open File Dialog 1
        /// </summary>
        /// <param name="sender">object sender</param>
        /// <param name="e">CancelEventArgs e</param>
        private void ofdDialog1_FileOk(object sender, CancelEventArgs e)
        {
            /**********************************************
				We only accept the selected and existed files
			**********************************************/
            if (!e.Cancel && ofdDialog1.CheckFileExists)
            {
                txtHugeSqlScript.Text = ofdDialog1.FileName.Trim();
            }
        }

        /// <summary>
        /// This event is raised up when the users choose file in Open File Dialog 2
        /// </summary>
        /// <param name="sender">object sender</param>
        /// <param name="e">CancelEventArgs e</param>
        private void ofdDialog2_FileOk(object sender, CancelEventArgs e)
        {
            /**********************************************
				We only accept the selected and existed files
			**********************************************/
            if (!e.Cancel && ofdDialog2.CheckFileExists)
            {
                txtLogSqlScript.Text = ofdDialog2.FileName.Trim();
            }
        }

        /// <summary>
        /// This event is raised up when the user clicks Run button
        /// </summary>
        /// <param name="sender">object sender</param>
        /// <param name="args">EventArgs args</param>
        private void btnRun_Click(object sender, EventArgs args)
        {
            /**********************************************
				If the form is valid and background worker
				isn't busy, we will run the script
				file asynchronously
			**********************************************/
            if (!IsValidForm()) return;

            if (!bwRunBigSqlScript.IsBusy)
            {
                bwRunBigSqlScript.RunWorkerAsync();
            }
        }

        /// <summary>
        /// This event is raised up when the user clicks More Connection String link
        /// </summary>
        /// <param name="sender">object sender</param>
        /// <param name="e">LinkLabelLinkClickedEventArgs e</param>
        private void lbMoreConnectionString_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            /**********************************************
				Navigate to the web site link to provide
				more information about connection strings
			**********************************************/
            lbMoreConnectionString.LinkVisited = true;
            Process.Start("http://connectionstrings.com/");
        }

        /// <summary>
        /// This event is raised up when the user checks into "Enable log to the file"
        /// </summary>
        /// <param name="sender">object sender</param>
        /// <param name="e">EventArgs e</param>
        private void cbEnableLog_CheckedChanged(object sender, EventArgs e)
        {
            /**********************************************
				Enable/Disable the Log Sql Script Textbox
				and the Choose File 2
			**********************************************/
            txtLogSqlScript.Enabled = btnChooseFile2.Enabled = cbEnableLog.Checked;
        }

        /// <summary>
        /// This event is raised up when the user cancels the running process
        /// </summary>
        /// <param name="sender">object sender</param>
        /// <param name="e">EventArgs e</param>
        private void btnCancel_Click(object sender, EventArgs e)
        {
            /**********************************************
				Cancel the asynchronous operation if current
				worker supports cancellation
			**********************************************/
            if (bwRunBigSqlScript.WorkerSupportsCancellation)
            {
                bwRunBigSqlScript.CancelAsync();
            }
        }

        /// <summary>
        /// This event is raised up when the user runs asynchronously
        /// </summary>
        /// <param name="sender">object sender</param>
        /// <param name="e">DoWorkEventArgs e</param>
        private void bwRunBigSqlScript_DoWork(object sender, DoWorkEventArgs e)
        {
            /**********************************************
				Measure time of running sql script
			**********************************************/
            var stopWatch = new Stopwatch();
            stopWatch.Start();

            /**********************************************
				Initialize as start
			**********************************************/
            var connectionString = txtConnectionString.Text.Trim();
            var enabledLogToFile = false;
            TextWriter logger = null;
            rtbStatus.Invoke(new MethodInvoker(delegate { rtbStatus.Text = string.Empty; }));
            ShowInProgress(false);

            try
            {
                /**********************************************
					Open the connection to the server database 
					if okay
				**********************************************/
                var connection = new SqlConnection(connectionString);
                connection.Open();

                /**********************************************
					Only accept the opening status to database
				**********************************************/
                if (connection.State != ConnectionState.Open) return;
                /**********************************************
						Initialize the file name path
					**********************************************/
                var fileNamePath = txtHugeSqlScript.Text.Trim();

                /**********************************************
						Enable to write log message to the file
					**********************************************/
                enabledLogToFile = cbEnableLog.Checked;
                if (enabledLogToFile)
                {
                    logger = GetLogFile(txtLogSqlScript.Text.Trim());
                    enabledLogToFile = logger != null;
                    if (enabledLogToFile)
                    {
                        logger.WriteLine($"*************** [{DateTime.Now}]****************");
                        logger.WriteLine($"Running {fileNamePath}...");
                    }
                }

                /**********************************************
						Open new stream reader to the script file
					**********************************************/
                var fileReader = new StreamReader(fileNamePath);

                /**********************************************
						Contain line code sql script and next line
						one
					**********************************************/
                string nextScriptDataLine;
                var scriptDataline = nextScriptDataLine = string.Empty;

                /**********************************************
						Count the number of records added to db
					**********************************************/
                var counter = 0;

                /**********************************************
						Count the number of affected when running
						command sql
					**********************************************/

                /**********************************************
						Initialize the Sql Command to run the sql
						script
					**********************************************/

                /**********************************************
						Initialize the message to show to the user
					**********************************************/

                /**********************************************
						Specify the first times to run the script
					**********************************************/
                var isFirst = true;

                /**********************************************
						Indicate the running status
					**********************************************/
                rtbStatus.Invoke(new MethodInvoker(delegate { rtbStatus.AppendText("Running...\n"); }));

                /**********************************************
						Indicate running the script until EOF
					**********************************************/
                while ((nextScriptDataLine = fileReader.ReadLine()) != null)
                {
                    try
                    {
                        /**********************************************
								If true, we will cancel the running thread
								otherwise will continue running
							**********************************************/
                        if (bwRunBigSqlScript.CancellationPending)
                        {
                            /**********************************************
									Release these unnecessary resources when
									cancelled
								**********************************************/
                            e.Cancel = true;
                            fileReader.Close();
                            connection.Close();
                            break;
                        }

                        /**********************************************
									Should combine the current line and the new
									line which was read from file to run
								**********************************************/
                        scriptDataline = $"{scriptDataline} {nextScriptDataLine}";

                        /**********************************************
									Should trim the line data to avoid extra
									whitespaces between begin and end string
								**********************************************/
                        scriptDataline = scriptDataline.Trim();

                        /**********************************************
									Run the script right away when seeing GO
									batch. This batch is always sensitive case
								**********************************************/
                        if (!scriptDataline.EndsWith("GO")) continue;
                        /**********************************************
										Cut off the GO string at the end of the line
										data
									**********************************************/
                        scriptDataline = scriptDataline.Substring(0, scriptDataline.Length - 2);

                        /**********************************************
										Send the script to the database server to
										run
									**********************************************/
                        var sqlCommand = new SqlCommand(scriptDataline, connection);
                        var numberOfAffectedRows = sqlCommand.ExecuteNonQuery();

                        /**********************************************
										Reset the line data to blank value
									**********************************************/
                        scriptDataline = string.Empty;

                        /**********************************************
										If having the number of affected rows is
										greater than zero, we will add it into
										counter
									**********************************************/
                        if (numberOfAffectedRows > 0)
                        {
                            counter += numberOfAffectedRows;
                        }

                        /**********************************************
										Only accept when the counter is greater
										than zero
									**********************************************/
                        if (counter <= 0) continue;
                        /**********************************************
											If this is the first time, we will append
											message to Rich Text Box to update status,
											otherwise, we will replace the counter value
											into message existed in Rich Text Box
										**********************************************/
                        string message;
                        if (isFirst)
                        {
                            message = $"Added {counter} row(s)";
                            rtbStatus.AppendText(message);
                            isFirst = false;
                        }
                        else
                        {
                            message = $"Added {counter - numberOfAffectedRows} row(s)";
                            if (!rtbStatus.Text.Contains(message)) continue;
                            rtbStatus.Text = rtbStatus.Text.Replace(message, $"Added {counter} row(s)");
                        }
                    }
                    catch (SqlException ex)
                    {
                        WriteExceptionError(ex, logger, enabledLogToFile);
                        scriptDataline = string.Empty;
                    }
                    catch (Exception ex)
                    {
                        WriteExceptionError(ex, logger, enabledLogToFile);
                        break;
                    }
                }

                /**********************************************
						Confirm that the sql script file were
						finished running. If enabling the log, we
						will write the current status to the log
						file
					**********************************************/
                if (enabledLogToFile)
                {
                    logger.WriteLine("Completed");
                    logger.WriteLine($"Total {counter} rows added to database");
                }
                rtbStatus.AppendText("\nCompleted\n");
                rtbStatus.AppendText($"Total {counter} rows added to database\n");

                /**********************************************
						Release unused resources
					**********************************************/
                fileReader.Close();
                connection.Close();
            }
            catch (Exception ex)
            {
                WriteExceptionError(ex, logger, enabledLogToFile);
            }
            finally
            {
                /**********************************************
					Release unused logger
				**********************************************/
                if (enabledLogToFile)
                {
                    logger.Close();
                }

                /**********************************************
					Stop watch and write the elapsed time to
					Rich Text Box
				**********************************************/
                stopWatch.Stop();
                var timeSpan = stopWatch.Elapsed;
                var elapsedTime =
                    $"Time elapsed: {timeSpan.Hours:00}:{timeSpan.Minutes:00}:{timeSpan.Seconds:00}.{timeSpan.Milliseconds / 10:00}";

                rtbStatus.Invoke(new MethodInvoker(delegate { rtbStatus.AppendText(elapsedTime); }));
            }
        }

        /// <summary>
        /// This event is raised up when the background worker has completed
        /// </summary>
        /// <param name="sender">object sender</param>
        /// <param name="e">RunWorkerCompletedEventArgs e</param>
        private void bwRunBigSqlScript_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ShowInProgress(true);
        }

        /// <summary>
        /// This event is raised up when user clicks to copy example connection string
        /// </summary>
        /// <param name="sender">object sender</param>
        /// <param name="e">LinkLabelLinkClickedEventArgs e</param>
        private void lbCopyConnectionString_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Clipboard.SetText(lblExampleConnectionString.Text.Replace("e.g:", string.Empty).Trim());
        }
        #endregion Events

        #region Useful methods
        /// <summary>
        /// Enable to show the status of running the script file
        /// </summary>
        /// <param name="isEnable">bool isEnable</param>
        private void ShowInProgress(bool isEnable)
        {
            /**********************************************
				When running the script file, we will
				disable Run and enable Cancel button,
				otherwise, enable Run and disable Cancel
			**********************************************/
            btnRun.Invoke(new MethodInvoker(delegate { btnRun.Enabled = isEnable; }));
            btnCancel.Invoke(new MethodInvoker(delegate { btnCancel.Enabled = !isEnable; }));

            /**********************************************
				Use async to visible the progress bar
			**********************************************/
            void Invoker() => progressBar1.Visible = !isEnable;

            progressBar1.BeginInvoke((MethodInvoker)Invoker);
        }

        /// <summary>
        /// Checks the form is valid or not?
        /// </summary>
        /// <returns>Return true if it is valid, otherwise false</returns>
        private bool IsValidForm()
        {
            var builder = new StringBuilder();

            /**********************************************
				Check the Connection String textbox
			**********************************************/
            if (string.IsNullOrEmpty(txtConnectionString.Text.Trim()))
            {
                builder.AppendLine("The connection string field is required");
            }

            /**********************************************
				Check the Sql Script File Path textbox
			**********************************************/
            if (string.IsNullOrEmpty(txtHugeSqlScript.Text.Trim()))
            {
                builder.AppendLine("The big sql file path field is required");
            }
            else
            {
                if (!File.Exists(txtHugeSqlScript.Text.Trim()))
                {
                    builder.AppendLine("The big sql file hasn't existed");
                }
            }

            /**********************************************
				Check the Enable Log checkbox
			**********************************************/
            if (cbEnableLog.Checked)
            {
                if (string.IsNullOrEmpty(txtLogSqlScript.Text.Trim()))
                {
                    builder.AppendLine("The log file path field is required");
                }
                else
                {
                    if (Path.GetExtension(txtLogSqlScript.Text.Trim()).ToLower() != ".txt")
                    {
                        builder.AppendLine("The log file must contain the extension '.txt'");
                    }
                }
            }

            /**********************************************
				if the length of data of builder identifier
				is greater than 0, we will show message box
				errors to user
			**********************************************/
            if (builder.ToString().Length > 0)
            {
                MessageBox.Show(builder.ToString(), "Errors", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return builder.ToString().Length <= 0;
        }

        /// <summary>
        /// Gets and initializes the log file
        /// </summary>
        /// <param name="logFileNamePath">string logFileNamePath</param>
        /// <returns>Return the TextWriter instance if found, otherwise null</returns>
        private TextWriter GetLogFile(string logFileNamePath)
        {
            if (string.IsNullOrEmpty(logFileNamePath) || Path.GetExtension(logFileNamePath).ToLower() != ".txt")
                throw new Exception("The logging file name path is required and must contain the extension '.txt'");
            var fileStream = !File.Exists(logFileNamePath) ? File.Create(logFileNamePath) : new FileStream(logFileNamePath, FileMode.Open);
            return new StreamWriter(fileStream);
        }

        /// <summary>
        /// Write error to the log file and Rich Text Box
        /// </summary>
        /// <param name="ex">Exception ex</param>
        /// <param name="logger">TextWriter logger</param>
        /// <param name="enabledLogToFile">bool enabledLogToFile</param>
        private void WriteExceptionError(Exception ex, TextWriter logger, bool enabledLogToFile)
        {
            /**********************************************
				If enabling the log, we will write the error
				in Exception to the log file
			**********************************************/
            if (enabledLogToFile)
            {
                logger?.WriteLine(ex.Message);
            }

            /**********************************************
				If the message existed in Rich Text Box,
				we will do nothing, otherwise will append
				this error to Rich Text Box
			**********************************************/
            var buttontext = string.Empty;
            rtbStatus.Invoke(new MethodInvoker(delegate { buttontext = rtbStatus.Text; }));

            if (!buttontext.Contains(ex.Message))
            {
                rtbStatus.Invoke(new MethodInvoker(delegate { rtbStatus.AppendText(ex.Message + "\n"); }));
            }
        }
        #endregion Useful methods
    }
}
