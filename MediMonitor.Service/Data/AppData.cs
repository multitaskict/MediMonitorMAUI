using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

using MediMonitor.Service.Interfaces;
using MediMonitor.Service.Models;

using SQLite;

namespace MediMonitor.Service.Data
{
    /// <summary>
    /// Database access for SQLite.
    /// </summary>
	public class AppData
    {
        private SQLiteAsyncConnection database;

        private readonly string appVersion;

        /// <summary>
        /// Get the path of the database file.
        /// </summary>
        public string FilePath { get; }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="filePath">The path to the database file.</param>
        /// <param name="appVersion">The current version of the application.</param>
        public AppData(string filePath, string appVersion)
        {
            FilePath = filePath;
            this.appVersion = appVersion;

            //Init();
        }

        /// <summary>
        /// Create the database.
        /// </summary>
        /// <exception cref="NewVersionException">The application has been updated.</exception>
        public async Task Init()
        {
            database = new SQLiteAsyncConnection(new SQLiteConnectionString(FilePath, true));

            await CreateTables();

            var lastSavedVersion = LatestSavedVersion();
            if (lastSavedVersion == string.Empty)
            {
                await database.InsertAsync(new AppVersion { Version = appVersion });
            }
            else if (lastSavedVersion != appVersion)
            {
                await database.InsertAsync(new AppVersion { Version = appVersion });
            }
        }

        /// <summary>
        /// Create the tables in the database using reflection.
        /// </summary>
        private async Task CreateTables()
        {
            var assembly = GetType().Assembly;
            var tableTypes = assembly.GetTypes().Where(type => type.IsClass && type.GetInterfaces().Contains(typeof(IEntity)));
            var dbType = database.GetType();
            var createTable = dbType.GetMethod(nameof(database.CreateTableAsync), new[] { typeof(Type), typeof(CreateFlags) });

            foreach (var type in tableTypes)
            {
                var task = (Task)createTable.Invoke(database, new object[] { type, CreateFlags.AllImplicit });
                await task;
            }
        }

        /// <summary>
        /// Get the latest saved App version.
        /// </summary>
        /// <returns>The latest saved app version as string.</returns>
        public string LatestSavedVersion()
        {
            var table = database.Table<AppVersion>();

            var task = table.OrderByDescending(a => a.Version).Take(1).FirstOrDefaultAsync();

            task.Wait();

            return task.Result?.Version ?? string.Empty;
        }

        /// <summary>
        /// Get a list of items filtered by <paramref name="expression"/>.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <param name="expression">The where expression to filter the result with.</param>
        /// <returns>A list containing the items.</returns>
        /// <seealso cref="ToListAsync{T, U}(Expression{Func{T, U}}, bool, Expression{Func{T, bool}})"/>
        /// <seealso cref="GetByIdAsync{T}(int)"/>
        public async Task<List<T>> ToListAsync<T>(Expression<Func<T, bool>> expression = null)
            where T : class, IEntity, new()
        {
            var table = database.Table<T>();

            if (expression != null)
            {
                table = table.Where(expression);
            }

            return await table.ToListAsync();
        }

        /// <summary>
        /// Get a list of items ordered by <paramref name="orderExpression"/>, optionally filtered by <paramref name="expression"/>.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <typeparam name="U">The type of the order value.</typeparam>
        /// <param name="orderExpression">The expression to order with.</param>
        /// <param name="orderDescending">Should the order be descending?</param>
        /// <param name="expression">The where expression to filter the result with.</param>
        /// <returns>A list containing the items.</returns>
        /// <seealso cref="ToListAsync{T}(Expression{Func{T, bool}})"/>
        /// <seealso cref="GetByIdAsync{T}(int)"/>
        public async Task<List<T>> ToListAsync<T, U>(Expression<Func<T, U>> orderExpression, bool orderDescending = false, Expression<Func<T, bool>> expression = null)
            where T : class, IEntity, new()
        {
            var table = database.Table<T>();

            if (orderDescending)
            {
                table = table.OrderByDescending(orderExpression);
            }
            else
            {
                table = table.OrderBy(orderExpression);
            }

            if (expression != null)
            {
                table = table.Where(expression);
            }

            return await table.ToListAsync();
        }

        /// <summary>
        /// Get the Table query for the specified Entity.
        /// </summary>
        /// <typeparam name="T">The type of the entity.</typeparam>
        /// <returns>The <see cref="AsyncTableQuery{T}"/> for the table.</returns>
        public AsyncTableQuery<T> TableQuery<T>()
            where T : class, IEntity, new()
        {
            return database.Table<T>();
        }

        /// <summary>
        /// Get an <typeparamref name="T"/> entity by specifying it's <paramref name="id"/>.
        /// </summary>
        /// <typeparam name="T">The type of the entity you want to get.</typeparam>
        /// <param name="id">The id of the entity you want to get.</param>
        /// <returns>The entity if found, otherwise null</returns>
        /// <seealso cref="ToListAsync{T}(Expression{Func{T, bool}})"/>
        /// <seealso cref="ToListAsync{T, U}(Expression{Func{T, U}}, bool, Expression{Func{T, bool}})"/>
        public async Task<T> GetByIdAsync<T>(int id)
            where T : class, IEntity, new()
        {
            return (await ToListAsync<T>(t => t.Id == id)).FirstOrDefault();
        }

        /// <summary>
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="expression"></param>
        /// <returns></returns>
        public async Task<T> FirstOrDefaultAsync<T>(Expression<Func<T, bool>> expression)
            where T : class, IEntity, new()
        {
            return await database.Table<T>().Where(expression).FirstOrDefaultAsync();
        }

        /// <summary>
        /// Adds or Updates the entity into the database.
        /// </summary>
        /// <typeparam name="T">The type of the entity you want to insert/update</typeparam>
        /// <param name="entity">The entity you want to insert/update</param>
        /// <returns>The amount of rows updated.</returns>
        public async Task<int> SaveAsync<T>(T entity)
            where T : class, IEntity, new()
        {
            if (entity.Id > 0)
            {
                //Existing entity
                return await database.UpdateAsync(entity);
            }
            else
            {
                //New entity, insert.
                return await database.InsertAsync(entity);
            }
        }

        /// <summary>
        /// Delete the specified entity from the database.
        /// </summary>
        /// <typeparam name="T">The type of the entity you want to delete.</typeparam>
        /// <param name="entity">The entity you want to delete.</param>
        /// <returns>The number of rows deleted.</returns>
        public async Task<int> DeleteAsync<T>(T entity)
            where T : class, IEntity, new()
        {
            return await database.DeleteAsync(entity);
        }

        /// <summary>
        /// Delete and recreate the database
        /// </summary>
        /// <returns></returns>
        public async Task Recreate()
        {
            //Disconnect from the database
            await database.CloseAsync();

            //Delete the database file
            File.Delete(FilePath);

            //Recreate the database
            database = new SQLiteAsyncConnection(FilePath);

            await Init();
        }


        /// <summary>
        /// Get a specified user.
        /// </summary>
        /// <param name="userId">The id of the user to get</param>
        /// <returns>The requested user</returns>
        public User GetUser(int userId)
        {
            var table = database.Table<User>();

            var userTask = table.Where(u => u.Id == userId).FirstOrDefaultAsync();

            userTask.Wait();

            return userTask.Result;
        }

    }
}

