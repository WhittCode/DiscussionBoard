
using System;
using System.Collections.Generic;
using System.Linq;
using DiscussionBoard.Authorization;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace DiscussionBoard.Pages
{
    /// <summary>
    /// Logs PageModel for displaying application logs.
    /// </summary>
    [Authorize(Roles = ApplicationRoles.Admin)]
    public class Logs : PageModel
    {
        private readonly ILogger<Logs> _logger;
        private readonly ILogReader _logReader;

        [BindProperty(SupportsGet = true)]
        public string Level { get; set; }

        public List<LogEntry> Entries { get; set; }

        public Logs(ILogger<Logs> logger, ILogReader logReader)
        {
            _logger = logger;
            _logReader = logReader;

            Level = string.Empty;
            Entries = new List<LogEntry>();
        }

        /// <summary>
        /// Load the logs and display them, filtering if requested.
        /// </summary>
        public IActionResult OnGet()
        {
            try
            {
                LoadLogs();
            }
            catch
            {
                return RedirectToPage("/Error");
            }

            FilterLogs();

            return Page();
        }

        private void LoadLogs()
        {
            try
            {
                Entries = _logReader.Read().ToList();
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Unable to load logs");
                throw;
            }
        }

        private void FilterLogs()
        {
            if (string.IsNullOrEmpty(Level))
            {
                return;
            }

            for (int i = Entries.Count - 1; i >= 0; i--)
            {
                if (!string.Equals(
                        Entries[i].Level,
                        Level,
                        StringComparison.OrdinalIgnoreCase))
                {
                    Entries.RemoveAt(i);
                }
            }
        }
    }
}
