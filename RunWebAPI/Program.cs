using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebAPI;
using WebApiTestDb.Model;

namespace RunWebAPI
{
    class Program
    {
        private static Dictionary<int, string> _tokenDict = new Dictionary<int, string>();
        private static readonly HttpClient client = new HttpClient();
        private const string baseUrl = "https://localhost:5201/api/User";

        static async Task Main(string[] args)
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Loop until the user chooses to exit
            bool exit = false;
            while (!exit)
            {
                // Display menu options
                Console.WriteLine("\nSelect an option:\n1. Create User\n2. Retrieve User\n3. Update User\n4. Delete User\n5. Get active Users\n6. Get All Users including the deleted ones\n7. Exit");
                string option = Console.ReadLine();

                switch (option)
                {
                    case "1":
                        await CreateUser();
                        break;
                    case "2":
                        await RetrieveUser();
                        break;
                    case "3":
                        await UpdateUser();
                        break;
                    case "4":
                        await DeleteUser();
                        break;
                    case "5":
                        await RetrieveAllUsers();
                        break;
                    case "6":
                        await RetrieveAllUsers(includeDeletedUsers:true);
                        break;
                    case "7":
                        exit = true;
                        break;
                    default:
                        Console.WriteLine("Invalid option. Please try again.");
                        break;
                }
            }
        }

        private static async Task CreateUser()
        {
            // Get user input for the new user record
            Console.WriteLine("\nEnter the name of the new user:");
            string name = Console.ReadLine();

            Console.WriteLine("Enter the email address of the new user:");
            string email = Console.ReadLine();

            Console.WriteLine("Enter the password of the new user:");
            string password = Console.ReadLine();

            Console.WriteLine("Enter the info of the new user:");
            string info = Console.ReadLine();

            Console.WriteLine("Enter the id of the new user:");
            bool isValidId = int.TryParse(Console.ReadLine(), out int id);

            while(!isValidId)
            {
                Console.WriteLine("Enter a valid id for the new user:");
                isValidId = int.TryParse(Console.ReadLine(), out id);
            }

            // Create a new user record
            var newUser = new User
            {
                Username = name,
                Email = email,
                Password = password,
                Info = info,
                Id = id
            };

            var myContent = JsonConvert.SerializeObject(newUser);
            var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            using HttpResponseMessage postResponse = await client.PostAsync(baseUrl, byteContent);

            // If the response is successful, notify the user and capture the token
            if (postResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("User Created");
                var postResult = await postResponse.Content.ReadAsStringAsync();
                var jwtToken = JsonConvert.DeserializeObject<JwtToken>(postResult);
                if(_tokenDict.ContainsKey(id))
                {
                    _tokenDict[id] = jwtToken.Token;
                }
                else
                {
                    _tokenDict.Add(id, jwtToken.Token);
                }
            }
            else
            {
                Console.WriteLine($"\nFailed to create user. Please check the user info and try again.");
            }
        }

        private static async Task RetrieveUser()
        {
            // Get user input for the user ID to retrieve
            Console.WriteLine("\nEnter the ID of the user to retrieve:");
            string idString = Console.ReadLine();
            var isIdValid = int.TryParse(idString, out int id);
            var _token = string.Empty;

            while (!isIdValid)
            {
                Console.WriteLine("\nEnter a valid ID of the user to retrieve:");
                isIdValid = int.TryParse(idString, out id);
            }

            if(_tokenDict.ContainsKey(id))
            {
                _token = _tokenDict[id];
            }

            // Set the media type header to JSON
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            // Set the JWT token in the authorization header
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);


            using var getUserResponse = await client.GetAsync($"{baseUrl}/{id}");
            var getUserContent = await getUserResponse.Content.ReadAsStringAsync();

            // If the response is successful, read the response body and display the user record
            if (getUserResponse.IsSuccessStatusCode)
            {
                var user = JsonConvert.DeserializeObject<User>(getUserContent);
                Console.WriteLine($"\nActive User found:\nId: {user.Id}\nName: {user.Username}\nEmail: {user.Email}\nPassword: {user.Password}\nEmail: {user.Email}\nInfo: {user.Info}\nCreatedAt {user.CreatedAt}");
            }
            else
            {
                Console.WriteLine($"\nFailed to retrieve user: {getUserResponse.StatusCode}");
            }
        }

        private static async Task DeleteUser()
        {
            // Get user input for the user ID to delete
            Console.WriteLine("\nEnter the ID of the user to delete:");
            string idString = Console.ReadLine();
            var isIdValid = int.TryParse(idString, out int id);
            var _token = string.Empty;
            while (!isIdValid)
            {
                Console.WriteLine("\nEnter a valid ID of the user:");
                isIdValid = int.TryParse(idString, out id);
            }

            if (_tokenDict.ContainsKey(id))
            {
                _token = _tokenDict[id];
            }

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);

            //soft delete
            using var deleteResponse = await client.DeleteAsync($"{baseUrl}/{id}");

            // If the response is successful, display a success message
            if (deleteResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("\nUser deleted.");
            }
            else
            {
                Console.WriteLine($"\nFailed to delete user: {deleteResponse.StatusCode}");
            }
        }

        private static async Task UpdateUser()
        {
            // Get user input for the user ID to update
            Console.WriteLine("Enter the id of the existing user:");
            string idString = Console.ReadLine();
            var isIdValid = int.TryParse(idString, out int id);
            var _token = string.Empty;
            while (!isIdValid)
            {
                Console.WriteLine("\nEnter a valid ID of the user:");
                isIdValid = int.TryParse(idString, out id);
            }

            if (_tokenDict.ContainsKey(id))
            {
                _token = _tokenDict[id];
            }

            // Get user input for the new user record
            Console.WriteLine("\nEnter the updated name of the existing user:");
            string name = Console.ReadLine();

            Console.WriteLine("Enter the updated email address of the existing user:");
            string email = Console.ReadLine();

            Console.WriteLine("Enter the updated password of the new user:");
            string password = Console.ReadLine();

            Console.WriteLine("Enter the updated info of the new user:");
            string info = Console.ReadLine();

            // Create a new user record
            var existingUser = new User
            {
                Username = name,
                Email = email,
                Password = password,
                Info = info,
                Id = id
            };

            var myContent = JsonConvert.SerializeObject(existingUser);
            var buffer = System.Text.Encoding.UTF8.GetBytes(myContent);
            var byteContent = new ByteArrayContent(buffer);
            byteContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
            using var putResponse = await client.PutAsync($"{baseUrl}/{existingUser.Id}", byteContent);
            var putResult = await putResponse.Content.ReadAsStringAsync();

            // If the response is successful, display a success message
            if (putResponse.IsSuccessStatusCode)
            {
                Console.WriteLine("\nUser updated.");
            }
            else
            {
                Console.WriteLine($"\nFailed to updated user: {putResponse.StatusCode}");
            }
        }

        private static async Task RetrieveAllUsers(bool includeDeletedUsers = false)
        {
            using var getUserResponse = await client.GetAsync(baseUrl);
            var getUserContent = await getUserResponse.Content.ReadAsStringAsync();

            // If the response is successful, read the response body and display the user record
            if (getUserResponse.IsSuccessStatusCode)
            {
                var users = JsonConvert.DeserializeObject<IEnumerable<User>>(getUserContent);

                if(!includeDeletedUsers)
                {
                    users = users.Where(x => !x.DeletedAt.HasValue);
                }

                foreach (var user in users)
                {
                    if (user.DeletedAt.HasValue)
                        Console.WriteLine($"\nInactive User found:\nId: {user.Id}\nName: {user.Username}\nEmail: {user.Email}\nPassword: {user.Password}\nEmail: {user.Email}\nInfo: {user.Info}\nCreatedAt {user.CreatedAt}");
                    else
                        Console.WriteLine($"\nActive User found:\nId: {user.Id}\nName: {user.Username}\nEmail: {user.Email}\nPassword: {user.Password}\nEmail: {user.Email}\nInfo: {user.Info}\nCreatedAt {user.CreatedAt}\nDeletedAt {user.DeletedAt}");
                }
            }
            else
            {
                Console.WriteLine($"\nFailed to retrieve user: {getUserResponse.StatusCode}");
            }
        }
    }
}
