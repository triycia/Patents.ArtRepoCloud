namespace Patents.ArtRepoCloud.Service.DataProviders.Uspto.Contracts.AttorneyAgents
{
    [Serializable]
    public class AttorneyAgentsContract
    {
        public AttorneyAgentsContract(string status, IEnumerable<Dictionary<string, AddressContract>> resultBag)
        {
            Status = status;
            AttorneyAgentsDictionary = resultBag.SelectMany(x => x).ToDictionary(x => x.Key, x => x.Value);
        }

        public string Status { get; }
        public Dictionary<string, AddressContract> AttorneyAgentsDictionary { get; }
    }
}