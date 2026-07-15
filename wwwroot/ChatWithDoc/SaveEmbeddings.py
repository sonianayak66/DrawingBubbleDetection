import urllib3
import chromadb
import sys
import json
import os
import logging

# Set up logging
log_file_path = 'script_log.txt'
logging.basicConfig(filename=log_file_path, level=logging.INFO, 
                    format='%(asctime)s - %(levelname)s - %(message)s')


def get_ollama_embedding(prompt_text, model="mxbai-embed-large"):
    """
    Send a request to the Ollama server to generate embeddings for the given text using urllib3.
    """
    try:
        # Define the URL for the Ollama API
        ollama_api_url = "http://192.168.0.118:11434/api/embeddings"

        # Define the request payload and headers
        payload = json.dumps({
            "model": model,
            "prompt": prompt_text
        })

        headers = {
            'Content-Type': 'application/json'
        }

        # Create a urllib3 PoolManager instance to manage connections
        http = urllib3.PoolManager()

        # Send a POST request to the Ollama API with a custom timeout
        response = http.request(
            'POST',
            ollama_api_url,
            body=payload,
            headers=headers,
            timeout=urllib3.Timeout(connect=10.0, read=30.0)  # Custom timeout
        )

        # Check if the request was successful (HTTP status 200)
        if response.status == 200:
            return json.loads(response.data.decode('utf-8'))  # Parse and return the response JSON
        else:
            logging.error(f"Error: Failed to get embedding. Status Code: {response.status}")
            return None
    except urllib3.exceptions.TimeoutError:
        logging.error(f"Request to Ollama API timed out for text: {prompt_text}")
        return None
    except urllib3.exceptions.RequestError as e:
        logging.error(f"Error making Ollama API request: {str(e)}")
        return None


def save_embeddings(pdf_name, texts, docDbKey):
    try:
        client = chromadb.PersistentClient(path="../ChromaDB")
        collection = client.get_or_create_collection(name="DocumentBrowserEmbeddings", metadata={
            "hnsw:batch_size": 10000,
        })

        # Check for empty or invalid texts
        if not texts or not isinstance(texts, list):
            logging.error(f"No valid text provided for file: {pdf_name} with docDbKey: {docDbKey}")
            print("Error saving Embeddings: No valid text provided to save in ChromaDB")
            return "Error saving Embeddings: No valid text provided to save in ChromaDB"

        # Generate embeddings
        ids = []  # List to hold unique IDs for each embedding
        for i, text in enumerate(texts):
            if not text:  # Check if the text is empty
                logging.warning(f"Skipping empty text at index {i} for file: {pdf_name}")
                continue  # Skip empty texts

            logging.info(f"Processing text: {text}")  # Log the text being processed
            
            # Call the Ollama API to get the embedding
            response = get_ollama_embedding(prompt_text=text)

            # Get the embedding from the response
            if response and 'embedding' in response:
                embedding = response['embedding']

                # Create a unique ID for each embedding
                unique_id = f"{docDbKey}##{i + 1}"
                ids.append(unique_id)

                # Save embedding to ChromaDB
                collection.add(ids=unique_id, embeddings=embedding, documents=text, 
                               metadatas=[{'pdf_name': pdf_name, 'docDbKey': docDbKey}])
                logging.info(f"Processing embedding: {embedding}")
            else:
                logging.error(f"Failed to get embedding for text at index {i}: {text}")           
        logging.info(f"Saved Embeddings for file: {pdf_name} with docDbKey: {docDbKey}")
        print("Saved Embeddings")
        return "Saved Embeddings"

    except Exception as e:
        logging.error(f"Error while saving embedding for file - {pdf_name}, error = {str(e)}")
        print("Error saving Embeddings: " + str(e))
        return "Error saving Embeddings: " + str(e)


def main():
    try:
        # Read the input JSON file path
        temp_file_path = sys.argv[1]

        # Open and read the JSON file
        with open(temp_file_path, 'r', encoding='utf-8') as file:
            data = json.load(file)

        file_name = data['file_name']
        doc_id = data['doc_id']
        text_list = data['texts']

        # Log the input data
        logging.info(f"Processing file: {file_name}, doc_id: {doc_id}, number of texts: {len(text_list)}")

        # Save embeddings
        save_embeddings(file_name, text_list, doc_id)

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
#import os
#
#
#
#def save_embeddings(pdf_name, texts, docDbKey):
#    try:
#        current_directory = os.getcwd()
#        log_file_path = 'script_log.txt'
#        client = chromadb.PersistentClient(path="../ChromaDB")
#        collection = client.get_or_create_collection(name="DocumentBrowserEmbeddings" , metadata={
#            # "hnsw:search_ef": 200,
#            # "hnsw:num_threads": 8,
#            # "hnsw:resize_factor": 2,
#            "hnsw:batch_size": 10000,
#            # "hnsw:sync_threshold": 1000000,
#        })
#
#        # Check for empty or invalid texts
#        if not texts or not isinstance(texts, list):
#            with open(log_file_path, 'a', encoding='utf-8', errors='replace') as log_file:
#                log_file.write(f"No valid text provided for file: {pdf_name} with docDbKey: {docDbKey}\n")
#            print("Error saving Embeddings: No valid text provided to save in ChromaDB")
#            return "Error saving Embeddings: No valid text provided to save in ChromaDB"
#
#        # Generate embeddings
#        ids = []  # List to hold unique IDs for each embedding
#        for i, text in enumerate(texts):
#            if not text:  # Check if the text is empty
#                with open(log_file_path, 'a', encoding='utf-8', errors='replace') as log_file:
#                    log_file.write(f"Skipping empty text at index {i} for file: {pdf_name}\n")
#                continue  # Skip empty texts
#
#            # Generate embedding for each text
#            response = ollama.embeddings(model="mxbai-embed-large", prompt=text)
#            embedding = response.get('embedding')
#
#            if embedding is not None:
#                # Create a unique ID for each embedding
#                unique_id = f"{docDbKey}##{i + 1}"
#                ids.append(unique_id)
#                collection.add(ids=unique_id, embeddings=embedding, documents=text, metadatas=[{'pdf_name': pdf_name, 'docDbKey': docDbKey}])
#            else:
#                with open(log_file_path, 'a', encoding='utf-8', errors='replace') as log_file:
#                    log_file.write(f"Failed to get embedding for text at index {i}: {text}\n")
#
#        with open(log_file_path, 'a', encoding='utf-8', errors='replace') as log_file:
#            log_file.write(f"Saved Embeddings for file: {pdf_name} with docDbKey: {docDbKey}\n")
#        print("Saved Embeddings")  
#        return "Saved Embeddings"
#
#    except Exception as e:
#        with open(log_file_path, 'a', encoding='utf-8', errors='replace') as log_file:
#            log_file.write(f"Error while saving embedding for file - {pdf_name}, error = {str(e)}\n")
#        print("Error saving Embeddings: " + str(e))
#        return "Error saving Embeddings: " + str(e)
#
#
## if __name__ == '__main__':
##     text_list = []
##     file_name = sys.argv[1]
##     doc_id = sys.argv[2]
##     for i, text in enumerate(sys.argv[3:]):
##         text_list.append(text) 
#
##     save_embeddings(file_name,text_list,doc_id)
#
#def main():
#    temp_file_path = sys.argv[1]
#    
#    # Open and read the JSON file
#    try:
#        with open(temp_file_path, 'r', encoding='utf-8') as file:
#            data = json.load(file)
#    
#        file_name = data['file_name']
#        doc_id = data['doc_id']
#        text_list = data['texts']
#    except Exception as e:
#        log_file_path = 'script_log.txt'
#        with open(log_file_path, 'a', encoding='utf-8', errors='replace') as log_file:
#            log_file.write(f"error while extracting data from json , error = {str(e)}\n")
#        print("Error saving Embeddings : Cannot extract JSON data")
#        return "Error saving Embeddings : Cannot extract JSON data"
#    
#    save_embeddings(file_name, text_list, doc_id)
#
#if __name__ == "__main__":
#    main()   

