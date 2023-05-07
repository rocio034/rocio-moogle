namespace MoogleEngine;

using System.Text.RegularExpressions;

public class TFIDF
{
    // Contiene la cadena de consulta
    private string query;
    // Contiene el vector TF de la consulta
    private Dictionary<string, double> vectorQuery = new Dictionary<string, double>{};
    // Contiene el vector TF de todos los documentos
    private Dictionary<string, Dictionary<string, int>> corpus = new Dictionary<string, Dictionary<string, int>>{};
    
    // Contiene un vector con todos los terminos
    private Dictionary<string, string[]> bagOfWords = new Dictionary<string, string[]>{};

    // Contiene un vector con todas las oraciones
    private Dictionary<string, string[]> bagOfSentences = new Dictionary<string, string[]>{};

    // Contiene el vector TF
    public Dictionary<string, Dictionary<string, double>> tf = new Dictionary<string, Dictionary<string, double>>{};

    // Contiene el vector IDF
    public Dictionary<string, double> idf = new Dictionary<string, double>();

    // Contiene el vector TFIDF
    public Dictionary<string, Dictionary<string, double>> tfidf = new Dictionary<string, Dictionary<string, double>>{};


    /*
        * Funcion para inicializar el proceso.
        * Se formatea el query y se cargan los ficheros del directorio Content
        */
    public TFIDF(string directory, string query) {
        this.query = query;
        this.formatQuery(query);
        // Obtener el listado de los ficheros que se encuentran en la carpeta Content
        string[] documents = Directory.GetFiles(@directory);

        // Se comprueba si existen ficheros en el directorio
        if(documents.Length > 0) {
            HashSet<string> hashSetContent = new HashSet<string>();
            // Se recorre el listado de ficheros que estan en el directorio
            foreach(string document in documents){
                string content = System.IO.File.ReadAllText(@document);
                string fileName = Path.GetFileName(document);
                // Se lee el contenido de cada fichero y se extraen las palabras
                string[] words = Tokenize(content);
                // Se crea una matriz donde la key es el nombre del fichero y el valor es el array de palabras contenidas en el
                bagOfWords[fileName] = words;

                // Obtengo las oraciones del documento
                
                char[] charsToTrim = { '.', ':', ';'};
                string[] sentences = content.Split(charsToTrim);
                bagOfSentences[fileName] = sentences;
                // Se crea una coleccion a partir del array de palabras de cada fichero
                HashSet<string> hashSetWords = new HashSet<string>(words);
                // Se concatenan todas las palabras para crear una unica matriz (documento, termino) 
                hashSetContent.UnionWith(hashSetWords);
            }

            // Se recorre el listado de documentos
            foreach(KeyValuePair<string, string[]> words in bagOfWords){
                // Se inicializa en 0 la existencia de cada palabra
                Dictionary<string, int> dictInit = hashSetContent.ToDictionary(h => h , h => 0);
                // Se actualiza la existencia de las palabras por cada documento
                foreach(string word in words.Value){
                    dictInit[word] += 1;
                }
                this.corpus[words.Key] = dictInit;
            }
        }

    }

    /*
        * Funcion que formatea el criterio de busqueda (query)
        * Inicializa el valor de vectorQuery con el calculo del TF del query
        */
    private void formatQuery(string query) {
        // Se extraen las palabras del texto y se almacenan en el array bagOfWordsQ
        string[] bagOfWordsQ = Tokenize(query);
        HashSet<string> hashSetQ = new HashSet<string>(bagOfWordsQ);
        // Inicializando en 0 las frecuencias en la matriz del query
        Dictionary<string, double> numOfWordsQ = hashSetQ.ToDictionary(h => h , h => 0.0);
        foreach (string word in bagOfWordsQ) {
            numOfWordsQ[word] += 1;
        }
        //Calculando el TF del Query
        foreach (KeyValuePair<string, double> word in numOfWordsQ) {
            this.vectorQuery[word.Key] = ((double)word.Value/bagOfWordsQ.Length);
        }
    }

    
    /*
        * Se calcula el TF por cada documento
        *
        * Se realiza una division entre la cantidad de apariciones y la cantidad de palabras del documento.
        * Con el objetivo de no priorizar a los documentos mas grandes, teniendo en cuenta por ejemplo que:
        * Un termino con una ocurrencia de 10 en un documento de 800 terminos es menos delevante que un termino
        * con una ocurrencia de 8 en un documento de 100 terminos
        */
    public Dictionary<string, Dictionary<string, double>> computeTF() {
        foreach(KeyValuePair<string, Dictionary<string, int>> words in this.corpus){
            int wordsOfDoc = bagOfWords[words.Key].Length;
            Dictionary<string, double> tfDict = new Dictionary<string, double>{};
            foreach(KeyValuePair<string, int> word in words.Value){
                tfDict[word.Key] = ((double)word.Value/wordsOfDoc);
                tf[words.Key] = tfDict;
            }
        }
        return tf;
    }

    /*
        * Permite calcular el IDF
        *
        * Se calcula a partir de log(numero total de documentos/numero de documentos en el que un termino esta presente)
        */
    public Dictionary<string, double> computeIDF() {
        int N = this.corpus.ToArray().Length;
        //Calculando el numero de documentos en  que esta presente cada termino
        foreach (KeyValuePair<string, Dictionary<string, int>> words in this.corpus) {
            Dictionary<string, double> tmp = new Dictionary<string, double>();
            // Se recorren todos los terminos del documento i
            foreach (KeyValuePair<string, int> word in words.Value) {
                // Se comprueba si el tf del termino en el documento i es diferente de 0 para incrementar el contador en el vector idf (que estamos creando) 
                if(word.Value > 0) {
                    if(!tmp.ContainsKey(word.Key)) {
                        // Se verifica si el termino existe en el vector idf (que estamos creando) para incrementarle el contador
                        if(idf.ContainsKey(word.Key)) {
                            idf[word.Key] += 1;
                        } else { // si el termino no existia en el vector idf (que estamos creando) se crea con valor 1
                            idf.Add(word.Key, 1);
                        }
                        tmp.Add(word.Key, 1);
                    }
                }
            }
        }

        // Se aplica la formula final del idf 
        foreach (KeyValuePair<string, double> word in idf) {
            idf[word.Key] = Math.Log10(N/word.Value);
        }

        return idf;
    }

    /*
        * Permite calcular el TFIDF
        *
        * Se calcula a partir de la multiplicacion de los vectores TF y el vector IDF
        */
    public Dictionary<string, Dictionary<string, double>> computeTFIDF() {
        // Obtengo el vector TF de todos los documentos
        this.computeTF();
        
        // Obtengo el vector IDF
        this.computeIDF();

        foreach (KeyValuePair<string, Dictionary<string, double>> words in tf) {
            Dictionary<string, double> tfidfDict = new Dictionary<string, double>{};
            foreach (KeyValuePair<string, double> word in words.Value) {
                // Se calcula el TFIDF de cada documento 
                tfidfDict[word.Key] = (double)word.Value * idf[word.Key];
            }
            // Se adiciona el TFIDF a la lista que contiene los TFIDF de todos los documentos
            tfidf[words.Key] = tfidfDict;
        }
        return tfidf;
    }

    /*
        * Permite calcular la similitud del coseno
        *
        * Este metodo nos permite asignar un score a cada documento en relacion al criterio de busqueda
        */
    public Dictionary<string, double> computeCOSSIM() {
        Dictionary<string, double> simOfDocs = new Dictionary<string, double>{};
        double v = 0;
        double w = 0;

        foreach (KeyValuePair<string, Dictionary<string, double>> words in tfidf) {
            double sim = 0;
            // Se calcula la sumatoria
            foreach (KeyValuePair<string, double> word in this.vectorQuery) {
                // Se realiza la sumatoria de la multiplicacion del termino i del vector query por el termino i del vector documento 
                sim += (double)word.Value * words.Value[word.Key]; //Producto punto 
                // Se realiza la sumatoria de los terminos del vector query al cuadrado
                v += Math.Pow(word.Value, 2);
            }

            // Se realiza la sumatoria de los terminos del vector documento al cuadrado
            foreach (KeyValuePair<string, double> word in words.Value) {
                w += Math.Pow(word.Value, 2);
            }

            double simResult = sim / (Math.Sqrt(v) * Math.Sqrt(w));
            if(simResult > 0)
                simOfDocs[words.Key] = simResult;
        }

        // Ordenando por relevancia los resultados
        Dictionary<string, double> simOrdered = simOfDocs.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value); // Ordena por valor
        return simOrdered;
    }

    /*
     * Crea un array con los resultados de la busqueda. 
     * Utiliza el algoritmo de la distancia de Levenshtein 
     * para determinar el fragmento de texto (snippet) 
     * que se mostrara por cada documento resultante.
     */
    public string[][] normalizeResult(Dictionary<string, double> simOfDocs) {
        string[][] normalizedResult = new string[simOfDocs.ToArray().Length][];int i = 0;

        foreach(KeyValuePair<string, double> docs in simOfDocs) {
            string[] sentences = this.bagOfSentences[docs.Key];
            int score = 10000000;
            double percent = 100f;
            string sentenceResult = "";
            foreach(string sentence in sentences) {
                if(sentence.Length > 0) {
                    double percentOut;
                    int dist = this.LevenshteinDistance(sentence, this.query, out percentOut);
                    
                    if(percentOut < percent) {
                        percent = percentOut;
                        score = dist;
                        sentenceResult = sentence;
                    }
                }
            }
            normalizedResult[i] = new string[] { docs.Key, sentenceResult, docs.Value+"" };
            i++;
        }
        
        return normalizedResult;
    }

    /*
     * Aplica expresiones regulares para eliminar caracteres especiales, numeros y cadenas no deseadas.
     * Fragmenta la cadena obteniendo un array de palabras.
     */
    private static string[] Tokenize(string text)
    {
        // Elimina caracteres asociados a HTML y XML.
        text = Regex.Replace(text, "<[^<>]+>", "");

        // Elimina los numeros.
        text = Regex.Replace(text, "[0-9]+", "number");

        // Elimina caracteres asociados a urls.
        text = Regex.Replace(text, @"(http|https)://[^\s]*", "httpaddr");

        // Elimina caracteres asociados a email.
        text = Regex.Replace(text, @"[^\s]+@[^\s]+", "emailaddr");

        // Elimina caracter de moneda.
        text = Regex.Replace(text, "[$]+", "dollar");

        // Fragmenta la cadena en palabras utilizando conjunto de caracteres como patron de separacion
        return text.Split(" @$/#.-:&*+=[]?!(){},''\">_<;%\\".ToCharArray());
    }

    /*
     * Calcula la distancia entre dos frases. Este metodo implementa el algoritmo de la distancia de Levenshtein.
     * Este metodo tambien devuelve el porcentaje de cambios.
     */
    public int LevenshteinDistance(string sentence, string query, out double percent)
    {
        // Se busca si el query se encuentra en la cadena se devuelve maxima coincidencia
        if(sentence.Contains(query)) {
            percent = 0;
            return 0;
        }
        percent = 0;
        
        // d es una tabla con m+1 renglones y n+1 columnas
        int cost = 0;
        int m = sentence.Length;
        int n = query.Length;
        int[,] d = new int[m + 1, n + 1]; 

        // Verifica que exista algo que comparar
        if (n == 0) return m;
        if (m == 0) return n;

        // Llena la primera columna y la primera fila.
        for (int i = 0; i <= m; d[i, 0] = i++) ;
        for (int j = 0; j <= n; d[0, j] = j++) ;
        
        // Recorre la matriz llenando cada unos de los pesos.
        // i columnas, j renglones
        for (int i = 1; i <= m; i++)
        {
            for (int j = 1; j <= n; j++)
            {       
                // si son iguales en posiciones equidistantes el peso es 0
                // de lo contrario el peso suma a uno.
                cost = (sentence[i - 1] == query[j - 1]) ? 0 : 1;  
                d[i, j] = System.Math.Min(System.Math.Min(d[i - 1, j] + 1,  //Eliminacion
                            d[i, j - 1] + 1),                             //Insercion 
                            d[i - 1, j - 1] + cost);                     //Sustitucion
            }
        }

    /// Calculamos el porcentaje de cambios en la palabra.
        if (sentence.Length > query.Length)
        percent = ((double)d[m, n] / (double)sentence.Length);
        else
        percent = ((double)d[m, n] / (double)query.Length); 
        return d[m, n]; 
    }
}