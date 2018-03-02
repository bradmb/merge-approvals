using System;
using System.Collections.Generic;
using System.Linq;
using MergeApprover.Models;
using RestSharp;

namespace MergeApprover
{
    public class GitLabManager : IGitLabManager
    {
        private readonly RestClient _client;
        private readonly ConfigurationOptions _options;

        /// <summary>
        /// Initializes and sets up our RestClient
        /// </summary>
        /// <param name="options"></param>
        public GitLabManager(ConfigurationOptions options)
        {
            _options = options;
            _client = new RestClient($"{_options.Host}/api/v4/");
        }

        /// <summary>
        /// Lists out all of the merge requests in an open state
        /// </summary>
        /// <returns></returns>
        private IEnumerable<MergeRequestItem> ListMergeRequests()
        {
            var request = new RestRequest("merge_requests", Method.GET);
            request.AddHeader("Private-Token", _options.PrivateToken);
            request.AddParameter("state", "opened");
            request.AddParameter("scope", "all");

            var response = _client.Execute<List<MergeRequestItem>>(request);
            return response.Data;
        }

        /// <summary>
        /// Approves the merge request
        /// </summary>
        /// <param name="item"></param>
        private void ApproveMergeRequest(MergeRequestItem item)
        {
            var request = new RestRequest($"projects/{item.Project_Id}/merge_requests/{item.IId}/approve", Method.POST);
            request.AddHeader("Private-Token", _options.PrivateToken);

            var response = _client.Execute(request);
        }

        /// <summary>
        /// Goes through any open merge request, checks to seee if the source and destination branches are
        /// allowable for auto-approval, and then approves those branches
        /// </summary>
        public void ProcessMergeRequests()
        {
            var mergeRequests = this.ListMergeRequests();
            foreach (var req in mergeRequests)
            {
                var wildcardDestinations = _options.AutoApprovedRequests.Where(b => b.Source.Contains("*") || b.Destination.Contains("*"));
                var normalDestinations = _options.AutoApprovedRequests.Where(b => !b.Source.Contains("*") && !b.Destination.Contains("*"));

                //
                // STEP 1: Standard non-wildcard branch matching
                //
                if (normalDestinations.Any(b => b.Source == req.Source_Branch && b.Destination == req.Target_Branch))
                {
                    this.ApproveMergeRequest(req);
                } else if (wildcardDestinations.Any()) {
                    //
                    // STEP 2: Wilcard branch matching (useful for cherry picks)
                    //
                    foreach (var wc in wildcardDestinations)
                    {
                        var source = wc.Source.Replace("*", String.Empty);
                        var destination = wc.Destination.Replace("*", String.Empty);

                        //
                        // First we're going to check and see our config has a source branch with a wildcard
                        // flag. If it does, then we'll do a .Contains() check on that branch to see if there's a
                        // match. If not, we'll check the destination branch and compare those.
                        //
                        if (wc.Source.Contains("*") && req.Source_Branch.Contains(source) && wc.Destination == req.Target_Branch) {
                            this.ApproveMergeRequest(req);
                        } else if (wc.Destination.Contains("*") && req.Target_Branch.Contains(destination) && wc.Source == req.Source_Branch)
                        {
                            this.ApproveMergeRequest(req);
                        }
                    }
                }
            }
        }
    }
}
