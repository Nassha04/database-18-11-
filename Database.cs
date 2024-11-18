using FirebaseAdmin;
using Google.Cloud.Firestore;
using Homework2Library;

namespace DatabaseLogic
{
    public class DatabaseAccountrix
    {
        private const string firebase_key = "accountrixfinal";
        private FirestoreDb database;

        //temporary lists are created to store newly retrieved data from Firebase
        public TransactionList tmp_trans { get; set; }
        public Inventory tmp_items { get; set; }

        public List<UserDetail> userDetails { get; set; }

        public DatabaseAccountrix()
        {
            tmp_trans = new TransactionList();
            tmp_items = new Inventory();

        }

        public void initDatabase()
        {
            FirebaseApp.Create();
            database = FirestoreDb.Create(firebase_key);
            Console.WriteLine("Created Cloud Firestore client with project ID: {0}", firebase_key);
        }

        //to save transaction
        public async Task saveTransaction(UserTransaction a_user_transaction)
        {
            CollectionReference collectionRef = database.Collection("user_transactions");
            DocumentReference docRef = collectionRef.Document(a_user_transaction.transaction_id); //DateTime.Now.Ticks.ToString() is used because it has near perfect unique ID
            Dictionary<string, object> transaction_data = new Dictionary<string, object>
            {
                {"TransactionReason", a_user_transaction.getTransactionReason()},
                {"Amount", a_user_transaction.getAmount()},
                //need to save transaction date as string because firebase only allows UTC timestamp, if save as DateTime need to convert to UTC before saving
                {"TransactionDate", a_user_transaction.getTransactionDateTime().ToString()},
                {"transaction_id", a_user_transaction.getTransactionID()}
            };

            Console.WriteLine("Transaction with ID: {0} has been saved.\n", docRef.Id);
            Console.WriteLine("Data saved: \n");
            Console.WriteLine("Transaction amount: {0}", a_user_transaction.getAmount());
            Console.WriteLine("Transaction reason: {0}", a_user_transaction.getTransactionReason());
            Console.WriteLine("Transaction date: {0}", a_user_transaction.getTransactionDateTime());
            await docRef.SetAsync(transaction_data);
        }

        //to save user items
        public async Task saveAsset(Item a_user_asset)
        {
            CollectionReference collectionRef = database.Collection("user_items");
            DocumentReference docRef = collectionRef.Document(a_user_asset.itemID);
            Dictionary<string, object> items = new Dictionary<string, object>
            {
                {"Item ID", a_user_asset.getItemID()},
                {"Item Name", a_user_asset.getItemName()},
                {"Quantity", a_user_asset.getItemCount()},
                {"Price", a_user_asset.getPrice()}
            };
            Console.WriteLine("User asset with ID: {0} has been saved.\n", a_user_asset.itemID);
            Console.WriteLine("Data saved: \n");
            Console.WriteLine("Item Name: {0}", a_user_asset.getItemName());
            Console.WriteLine("Item ID: {0}", a_user_asset.getItemID());
            Console.WriteLine("Quantity: {0}", a_user_asset.getItemCount());
            Console.WriteLine("Price: {0}", a_user_asset.getPrice());
            await docRef.SetAsync(items);
        }

        public async Task saveUserData(UserDetail a_user_detail)
        {
            CollectionReference collectionRef = database.Collection("user_detail");
            DocumentReference docRef = collectionRef.Document(a_user_detail.getUserID());
            Dictionary<string, object> userdata = new Dictionary<string, object>
            {
                {"user_ID", a_user_detail.getUserID()},
                {"email", a_user_detail.getEmail()},
                {"password", a_user_detail.getPassword()},
                {"name", a_user_detail.getName()},
            };
            Console.WriteLine("User detail with email {0} saved successfully", a_user_detail.getEmail());
            await docRef.SetAsync(userdata);
        }
        //to retrieve data from Firebase according to collection and it gets every document inside the collection and temporarily stores in in a dictionary
        //it is then distributed to temporary variables to store the value 
        //then it is transferred to a temporary object
        public async Task retrieveTransaction()
        {
            Query collectionQuery = database.Collection("user_transactions");
            QuerySnapshot allQuerySnapshot = await collectionQuery.GetSnapshotAsync();

            tmp_trans.Clear();
            /*
            code below can be interpreted as: allQuerySnapshot contains the collection snapshot, meaning all collection in the database, and of course, 
            we created documents for each collection, so naturally, allQuerySnapshot also contains the document of the collection itself,
            so to acces the documents in the collection, we will use foreach loop to access the documents inside the collection that we snapshotted.
            */
            foreach (DocumentSnapshot documentSnapshot in allQuerySnapshot.Documents) //using document snapshot to take info from document itself
            {
                Dictionary<string, object> data = documentSnapshot.ToDictionary(); //creating a dictionary called data and storing the dictionary transformed documentsnapshot into it.
                double tmp_amount = double.Parse(data["Amount"].ToString()); //stepping into the key of "Amount" and taking its value to store into tmp_amount.
                //since firebase can only accept timestamps in UTC format, we need to have these additional operations to convert the UTC time back to Local Time
                /*
                string tmp_timestamp = data["TransactionDate"].ToString();
                string tmp_cleanedtimestamp = tmp_timestamp.Replace("Timestamp: ", "");
                DateTime tmp_datetime_utc = DateTime.Parse(tmp_cleanedtimestamp);
                DateTime tmp_datetime_local = tmp_datetime_utc.ToLocalTime();
                */ //either this solution or just save it in string then convert back to DateTime
                DateTime tmp_date = DateTime.Parse(data["TransactionDate"].ToString());

                string tmp_transreason = data["TransactionReason"].ToString();
                string tmp_transaction_id = data["transaction_id"].ToString();
                //if use alternative solution replace with tmp_datetime_local
                UserTransaction tmp_transaction = new UserTransaction(tmp_amount, tmp_transreason, tmp_date, tmp_transaction_id);
                tmp_trans.AddTransaction(tmp_transaction);
            }
            Console.WriteLine("Data retrieved: \n");
            tmp_trans.DisplayTransactionHistory();
        }

        //retrieve user transaction and return as list

        //method to retrive user items from database
        public async Task retrieveItem()
        {
            Query collectionQuery = database.Collection("user_items");
            QuerySnapshot allQuerySnapshot = await collectionQuery.GetSnapshotAsync();

            tmp_items.Clear();
            foreach (DocumentSnapshot documentSnapshot in allQuerySnapshot.Documents)
            {
                Dictionary<string, object> data = documentSnapshot.ToDictionary();

                string tmp_itemID = data["Item ID"].ToString();
                string tmp_itemName = data["Item Name"].ToString();
                int tmp_quantity = int.Parse(data["Quantity"].ToString());
                double tmp_price = double.Parse(data["Price"].ToString());

                Item tmp_item = new Item(tmp_itemName, tmp_quantity, tmp_price, tmp_itemID);
                tmp_items.AddItem(tmp_item);
            }
            Console.WriteLine("Data retrived: \n");
            tmp_items.DisplayInventory();
        }

        //retrieve single document == user detail
        public async Task retrieveUserData(string a_user_ID)
        {
            //shorthand way of writing, got it from documentation.
            DocumentReference docRef = database.Collection("user_detail").Document(a_user_ID);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            Dictionary<string, object> data = snapshot.ToDictionary();
            string tmp_userID = data["user_ID"].ToString();
            string tmp_email = data["email"].ToString();
            string tmp_password = data["password"].ToString();
            string tmp_name = data["name"].ToString();

            UserDetail tmp_userdetail = new UserDetail(tmp_email, tmp_password, tmp_userID, tmp_name);
            Console.WriteLine("Data of user ID: {0} has been successfully retrieved.", tmp_userdetail.getUserID());
            tmp_userdetail.DisplayUserData();
        }

        public async Task<UserDetail> retrieveUserDataAsDoc()
        {
            Query collectionQuery = database.Collection("user_detail");
            QuerySnapshot allQuerySnapshot = await collectionQuery.GetSnapshotAsync();
            if (allQuerySnapshot.Count == 0)
            {
                return null;
            }
            else
            {
                Dictionary<string, object> data = allQuerySnapshot.Documents[0].ToDictionary();
                string tmp_userID = data["user_ID"].ToString();
                string tmp_email = data["email"].ToString();
                string tmp_password = data["password"].ToString();
                string tmp_name = data["name"].ToString();

                UserDetail tmp_userdetail = new UserDetail(tmp_email, tmp_password, tmp_userID, tmp_name);
                return tmp_userdetail;
            }
        }

        public async Task<UserDetail> retrieveUserEmail(string a_user_ID)
        {
            // Shorthand way of writing, got it from documentation.
            DocumentReference docRef = database.Collection("user_detail").Document(a_user_ID);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            if (snapshot.Exists)
            {
                Dictionary<string, object> data = snapshot.ToDictionary();

                string tmp_userID = data["user_ID"].ToString();
                string tmp_email = data["email"].ToString();
                string tmp_password = data["password"].ToString();
                string tmp_name = data["name"].ToString();

                UserDetail tmp_userdetail = new UserDetail(tmp_email, tmp_password, tmp_userID, tmp_name);

                Console.WriteLine("Data of user ID: {0} has been successfully retrieved.", tmp_userdetail.getUserID());
                tmp_userdetail.DisplayUserData();

                return tmp_userdetail;
            }
            else
            {
                Console.WriteLine("No user data found for the provided ID.");
                return null;
            }
        }

        //delete transaction with reference to tick time AKA documentID
        public async Task deleteTransaction(string a_user_ID)
        {
            CollectionReference collectionRef = database.Collection("user_transactions");
            DocumentReference docRef = collectionRef.Document(a_user_ID);

            Console.WriteLine("Transaction with ID: {0} has been deleted", a_user_ID);
            await docRef.DeleteAsync();
        }
        //delete user data => usually used for updating settings
        public async Task deleteUserData(string a_user_ID)
        {
            DocumentReference docRef = database.Collection("user_detail").Document(a_user_ID);
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();

            Console.WriteLine("User data with ID: {0} has been deleted", a_user_ID);
            await docRef.DeleteAsync();
        }
        //delete user item
        public async Task deleteAsset(string docID)
        {
            CollectionReference collectionRef = database.Collection("user_items");
            DocumentReference docRef = collectionRef.Document(docID);

            //getting the key value pairs of the item stored in database    
            DocumentSnapshot snapshot = await docRef.GetSnapshotAsync();
            Item tmp_item = snapshot.ConvertTo<Item>();

            Console.WriteLine("Item with ID: {0} has been deleted", tmp_item.getItemID());
            await docRef.DeleteAsync();
        }

        public async Task<UserDetail> retrieveUserByEmail(string email)
        {
            Query collectionQuery = database.Collection("user_detail").WhereEqualTo("email", email);
            QuerySnapshot allQuerySnapshot = await collectionQuery.GetSnapshotAsync();

            if (allQuerySnapshot.Count == 0)
            {
                return null;
            }

            Dictionary<string, object> data = allQuerySnapshot.Documents[0].ToDictionary();
            string tmp_userID = data["user_ID"].ToString();
            string tmp_email = data["email"].ToString();
            string tmp_password = data["password"].ToString(); // Do not display password in logs
            string tmp_name = data["name"].ToString();

            return new UserDetail(tmp_email, tmp_password, tmp_userID, tmp_name);
        }
    }

}

