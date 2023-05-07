namespace MoogleEngine;


public static class Moogle
{
    public static SearchResult Query(string query) {
        TFIDF tfidf = new TFIDF("../Content", query);
        tfidf.computeTFIDF();
        Dictionary<string, double> result = tfidf.computeCOSSIM();
        string[][] results = tfidf.normalizeResult(result);

        SearchItem[] items = new SearchItem[results.Length];
        for(int i = 0; i < results.Length; i++) {
            items[i] = new SearchItem(results[i][0], results[i][1], float.Parse(results[i][2]));
        }

        return new SearchResult(items, query);
    }
}
