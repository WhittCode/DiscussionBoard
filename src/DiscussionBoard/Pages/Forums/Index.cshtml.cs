using System.Collections.Generic;
using System.Threading.Tasks;
using DiscussionBoard.Models;
using DiscussionBoard.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace DiscussionBoard.Pages.Forums
{
    public class IndexModel : PageModel
    {
        private readonly ForumService _forums;
        private readonly ILogger<IndexModel> _logger;

        /// <summary>
        ///     The Forums page, displaying all active forums
        /// </summary>
        /// <param name="forums">Service to manage forums</param>
        /// <param name="logger">Logging provider</param>
        public IndexModel(ForumService forums, ILogger<IndexModel> logger)
        {
            _forums = forums;
            _logger = logger;
        }

        public List<Forum> Items { get; private set; } = [];

        /// <summary>
        ///     Returns a list of the active forums on the site
        /// </summary>
        public async Task OnGetAsync()
        {
            var result = await _forums.GetAllAsync();

            // Safe conversion if GetAllAsync returns an IEnumerable instead of a List
            Items = result is List<Forum> list ? list : new List<Forum>(result);
        }
    }
}


