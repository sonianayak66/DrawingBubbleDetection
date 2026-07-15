import urllib3
import chromadb
import sys
import json
import logging
import html
import tempfile


# Set up logging
log_file_path = 'script_log.txt'
logging.basicConfig(filename=log_file_path, level=logging.INFO, 
                    format='%(asctime)s - %(levelname)s - %(message)s')

client = chromadb.PersistentClient(path="../ChromaDB")
collection = client.get_or_create_collection(name="DocumentBrowserEmbeddings")

# Create a global http PoolManager instance to manage connections
# 172.30.249.9
http = urllib3.PoolManager()


def get_ollama_embedding(prompt_text, model="mxbai-embed-large"):
    """
    Send a request to the Ollama server to generate embeddings for the given text.
    """
    try:
        ollama_api_url = "http://172.30.249.9:11434/api/embeddings"  

        payload = json.dumps({
            "model": model,
            "prompt": prompt_text
        })

        headers = {
            'Content-Type': 'application/json'
        }
        response = http.request(
            'POST',
            ollama_api_url,
            body=payload,
            headers=headers,
            timeout=urllib3.Timeout(connect=10.0, read=60.0) 
        )

        logging.info(f"Ollama Embedding API Response Status: {response.status}")
        #logging.info(f"Ollama Embedding API Response Data: {response.data.decode('utf-8')}")

        if response.status == 200:
            return json.loads(response.data.decode('utf-8'))  
        else:
            logging.error(f"Error: Failed to get embedding. Status Code: {response.status}")
            return None
    except urllib3.exceptions.TimeoutError:
        logging.error(f"Request to Ollama API timed out for text: {prompt_text}")
        return None
    except urllib3.exceptions.RequestError as e:
        logging.error(f"Error making Ollama API request: {str(e)}")
        return None


def generate_response(user_prompt):
    try:
        with open(log_file_path, 'a', encoding='utf-8', errors='replace') as log_file:
            log_file.write(f"user prompt: {user_prompt}\n")
        data = []
        embedding_response = get_ollama_embedding(prompt_text=user_prompt)
     
        #logging.info(f"Embedding Response: {embedding_response}")

        if embedding_response is None or 'embedding' not in embedding_response:
            logging.error("Failed to generate embedding or missing 'embedding' key in response.")
            return {'error': 'Failed to generate embedding'}

        embedding = embedding_response['embedding']

        query_results = collection.query(query_embeddings=[embedding], n_results=5)
        logging.info(f"Query Results: {query_results}")

        # Set a distance threshold (e.g., 100, or any appropriate value based on your data)
        threshold = 1.0
        
        # Filter the results based on the distance threshold
        filtered_results = []
       
        for idx, distance in enumerate(query_results['distances'][0]):
            logging.info(f"distance: {distance}")
            if distance <= threshold:
                  # Make sure we are appending dictionaries with correct keys
                  result = {
                      'id': query_results['ids'][0][idx],
                      'distance': distance,
                      'metadata': query_results['metadatas'][0][idx],
                      'document': query_results['documents'][0][idx]
                  }
                  filtered_results.append(result)

        logging.info(f" filtered_results: {filtered_results}")
        if filtered_results:
            # Extract 'documents' and 'metadata' from all dictionaries in the list
            documents = [item['document'] for item in filtered_results]
            meta_Data = [item['metadata'] for item in filtered_results]
            print(f"docdbkey{meta_Data}",flush=True)
            logging.info(f"meta_Data: {meta_Data}")
        else:
            logging.error("No relevant documents found")
            print(f"NoRelevantDocFound",flush=True)
            return {'error': 'No relevant documents found'}

        previous_value = None
        generated_text = "<b>Referred Documents</b>:"
        for doc, meta in zip(documents, meta_Data):
            if meta is not None:
                logging.info(f"meta: {meta}")
                # Directly access the 'docDbKey' and 'pdf_name' from the meta dictionary
                if meta['docDbKey'] != previous_value:
                    generated_text += f'<a href="#" onclick="getDoc(\'{meta["docDbKey"]}\')">{meta["pdf_name"]}</a>'
                    generated_text += '<br>'
                previous_value = meta['docDbKey']
       
        if not documents or not any(documents):
            logging.error("No relevant documents found")
            return {'error': 'No relevant documents found'}

        ollama_generate_url = "http://172.30.249.9:11434/api/generate" 

        prompt_for_response = f"Answer the following query only based on the provided text extracts. Do not use any external knowledge or assumptions outside of what is given. Ensure that the answer is accurate, concise, and fully grounded in the provided information. Query:{user_prompt}. Provided Text Extracts: {documents}"
        #logging.info(f"prompt given: {prompt_for_response}")
        generate_payload = json.dumps({
            "model": "llama3.1:8b",
            "prompt": prompt_for_response,
            "stream": False  
        })

        response = http.request(
            'POST',
            ollama_generate_url,
            body=generate_payload,
            headers={'Content-Type': 'application/json'},
            timeout=urllib3.Timeout(connect=10.0, read=1200.0) 
        )

        if response.status == 200:
            generated_text_response = json.loads(response.data.decode('utf-8'))
            #generated_text += "ResposeData"
            generated_text += generated_text_response.get('response', '')
            generated_text = html.unescape(generated_text)     # to not to loose any html tag
            #print(json.dumps(generated_text))
            #data.append(gen_text)
            #data_string = json.dumps(data)
            #
            #with tempfile.NamedTemporaryFile(delete=False, suffix='.json') as temp_file:
            #    temp_file.write(data_string.encode())
            #    temp_file_path = temp_file.name
            #    print(temp_file_path)
            print(f"{generated_text}", flush=True)  # Use '|' as a delimiter
            #return generated_text
        else:
            logging.error(f"Error generating text. Status Code: {response.status} {response}")
            return None

    except Exception as e:
        logging.error(f"An error occurred: {str(e)}")
        return {'error': 'An error occurred while generating the response'}


def main():
    try:
      
        user_prompt = sys.argv[1]
        #with open(temp_file_path, 'r', encoding='utf-8', errors='replace') as file:
        #    data = json.load(file)
        #
        #user_prompt = data['texts']
    
     
        generate_response(user_prompt)

    except Exception as e:
        logging.error(f"Error while extracting data from JSON, error = {str(e)}")
        print("Error: Cannot extract JSON data")
        return "Error: Cannot extract JSON data"


if __name__ == "__main__":
    main()


#import ollama
#import chromadb
#import sys
#import json
#
#client = chromadb.PersistentClient(path="../ChromaDB")
#collection = client.get_or_create_collection(name="DocumentBrowserEmbeddings")
#
#def generate_response(user_prompt):
#    try:
#        log_file_path = 'script_log.txt'  # Change this to an absolute path if necessary
#        with open(log_file_path, 'a', encoding='utf-8', errors='replace') as log_file:
#            log_file.write(f"user prompt: {user_prompt}\n")
#    
#        # Generate embeddings for the user prompt
#        embedding_response = ollama.embeddings(prompt=user_prompt, model="mxbai-embed-large")
#        embedding = embedding_response.get('embedding')
#        if embedding is None:
#            print("Failed to generate embedding")
#            return {'error': 'Failed to generate embedding'}
#
#        query_results = collection.query(query_embeddings=[embedding], n_results=5)
#        with open(log_file_path, 'a', encoding='utf-8', errors='replace') as log_file:
#            log_file.write(f"query_results: {query_results}\n")
#        # Extract the relevant documents and metadata from the query results
#        documents = query_results.get('documents', [])
#        meta_Data = query_results.get('metadatas', [])
#        previous_value = None
#        generated_text = "<b>Referred Documents</b>:"
#        
#        for doc, meta in zip(documents, meta_Data):
#            if meta is not None:
#                for md in meta:
#                    if md['docDbKey'] != previous_value:
#                        generated_text += f'<a href="#" onclick="getDoc(\'{md["docDbKey"]}\')">{md["pdf_name"]}</a><br>'
#
#                    previous_value = md['docDbKey']
#        
#        if not documents or not any(documents):
#            print("Failed to generate embedding")
#            return {'error': 'No relevant documents found'}
#
#    
#        # Generate a response using the retrieved data
#        generated_text_response = ollama.generate(model="llama3.1:8b", prompt=f"Refer this as your data source: {documents}. Respond to this prompt: {user_prompt}")
#        generated_text += generated_text_response.get('response', '')
#        with open(log_file_path, 'a', encoding='utf-8', errors='replace') as log_file:
#            log_file.write(f"gen response: {generated_text}\n")
#        print(generated_text)
#        return generated_text
#
#    except Exception as e:
#        print(f"An error occurred: {e}")
#        return {'error': 'An error occurred while generating the response'}
#
#
#def main():
#    temp_file_path = sys.argv[1]
#    
#    # Open and read the JSON file
#    with open(temp_file_path, 'r', encoding='utf-8', errors='replace') as file:
#        data = json.load(file)
#    
#    user_prompt = data['texts']
#    
#    generate_response(user_prompt)
#
#if __name__ == "__main__":
#    main()   


# if __name__ == '__main__':
#     user_prompt = sys.argv[1]
#     generate_response(user_prompt)
#     #log_file_path = 'script_log.txt'
#     #with open(log_file_path, 'a') as log_file:
#      #   log_file.write(f"user_prompt: {sys.argv[1]}\n")
    
        