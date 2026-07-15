using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using MPCRS.Models;
using MPCRS.ViewModels;
using Newtonsoft.Json;
using System.Data;
using XAct;
using static MPCRS.Utilities.Constants;

namespace MPCRS.Utilities
{
    public static class DataCaching
    {
        private static readonly IMemoryCache _memoryCache;
        private static readonly MemoryCacheEntryOptions _memCacheOptions;

        static DataCaching()
        {
            try
            {
				_memoryCache = new MemoryCache(new MemoryCacheOptions());
				_memCacheOptions = new MemoryCacheEntryOptions
				{
					AbsoluteExpiration = DateTime.Now.AddDays(1),
					Priority = CacheItemPriority.High,
					SlidingExpiration = TimeSpan.FromDays(1)
				};
			}
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
            }
            
        }

        public static void removeCache(string cacheKey)
        {
            try
            {
				_memoryCache.Remove(cacheKey);
			}
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
            }
        }

        public static void RemoveAllCache()
        {
            try
            {
				foreach (CacheKeys value in Enum.GetValues(typeof(CacheKeys)))
				{
					string cacheKey = Enum.GetName(typeof(CacheKeys), value);
					removeCache(cacheKey.ToString());
				}
			}
            catch (Exception ex)
            {
                ErrorHandler.LogException(ex);
            }
        } 

        public static List<MetaMaster> getCachedMaster()
        {
            string cacheKey = CacheKeys.MetaMaster.ToString();

            if (!_memoryCache.TryGetValue(cacheKey, out List<MetaMaster> outputlist))
            {
                //calling the server
                try
                {
					using (DESI_STFE_PRODContext db = new())
					{
						outputlist = db.MetaMasters.AsNoTracking().ToList();
					}
					//setting cache entries
					_memoryCache.Set(cacheKey, outputlist, _memCacheOptions);
				}
                catch (Exception ex) { ErrorHandler.LogException(ex); }
            }
            return outputlist;
        }

        public static List<AspNetRoleClaim> getCachedRoleClaims()

        {
            string cacheKey = CacheKeys.RoleClaims.ToString();

            if (!_memoryCache.TryGetValue(cacheKey, out List<AspNetRoleClaim> outputlist))
            {
                //calling the server
                try
                {
					using (DESI_STFE_PRODContext db = new())
					{
						outputlist = db.AspNetRoleClaims.AsNoTracking().ToList();
					}
					//setting cache entries
					_memoryCache.Set(cacheKey, outputlist, _memCacheOptions);
				}
                catch (Exception ex) { ErrorHandler.LogException(ex); }
            }
            return outputlist;
        }

        public static List<AspNetRole> getCachedRole()
        {
            string cacheKey = CacheKeys.AspNetRoles.ToString();

            if (!_memoryCache.TryGetValue(cacheKey, out List<AspNetRole> outputlists))
            {
                //calling the server
                try
                {
					using (DESI_STFE_PRODContext db = new())
					{
						outputlists = db.AspNetRoles.AsNoTracking().ToList();
					}
					//setting cache entries
					_memoryCache.Set(cacheKey, outputlists, _memCacheOptions);
				}
                catch (Exception ex)
                {
                    ErrorHandler.LogException(ex);
                }
            }
            return outputlists;
        }

        public static List<PersonVM> getCachedPersonList()
        {
            string cacheKey = CacheKeys.Persons.ToString();

            if (!_memoryCache.TryGetValue(cacheKey, out List<PersonVM> outputlist))
            {
                try
                {
					//calling the server 
					DataTable dataTable = MPGlobals.GetDataForDatalist("[dbo].[PersonInfo_SSP]");
					string jsonResult = JsonConvert.SerializeObject(dataTable);
					outputlist = JsonConvert.DeserializeObject<List<PersonVM>>(jsonResult);
					//setting cache entries
					_memoryCache.Set(cacheKey, outputlist, _memCacheOptions);
				}
                catch (Exception ex)
                {
                    ErrorHandler.LogException(ex);
                }
            }
            return outputlist;
        }

        public static List<PersonVM> getCachedPersonListInactive()
        {
            string cacheKey = CacheKeys.Persons.ToString();

            if (!_memoryCache.TryGetValue(cacheKey, out List<PersonVM> outputlist))
            {
                try
                {
                    //calling the server 
                    DataTable dataTable = MPGlobals.GetDataForDatalist("[dbo].[PersonInfo_SSP] @UserStatus=0");
                    string jsonResult = JsonConvert.SerializeObject(dataTable);
                    outputlist = JsonConvert.DeserializeObject<List<PersonVM>>(jsonResult);
                    //setting cache entries
                    _memoryCache.Set(cacheKey, outputlist, _memCacheOptions);
                }
                catch (Exception ex)
                {
                    ErrorHandler.LogException(ex);
                }
            }
            return outputlist;
        }

        public static DataTable getCachedEngineParts()
        {
            string cacheKey = CacheKeys.AllEngineParts.ToString();

            if (!_memoryCache.TryGetValue(cacheKey, out DataTable outputlist))
            {
                try
                {
					outputlist = MPCRS.Utilities.Masters.GetEnginePartLists();
					_memoryCache.Set(cacheKey, outputlist, _memCacheOptions);
				}
                catch (Exception ex)
                {
                    ErrorHandler.LogException(ex);
                }
            }
            return outputlist;
        }

        public static List<ACSNvm> getCachedACSNList()
        {
            string cacheKey = CacheKeys.ACSNList.ToString();

            if (!_memoryCache.TryGetValue(cacheKey, out List<ACSNvm> outputlist))
            {
                try
                {
					DataTable dataTable = MPGlobals.GetDataForDatalist("exec dbo.Get_ACSN_List");
					string jsonResult = JsonConvert.SerializeObject(dataTable);
					outputlist = JsonConvert.DeserializeObject<List<ACSNvm>>(jsonResult);

					_memoryCache.Set(cacheKey, outputlist, _memCacheOptions);
				}
                catch (Exception ex)
                {
                    ErrorHandler.LogException(ex);
                }
            }
            return outputlist;
        }

        public static List<AcsnDashbord> getCachedDashborad()
        {
            string cacheKey = CacheKeys.dashborad.ToString();

            if (!_memoryCache.TryGetValue(cacheKey, out List<AcsnDashbord> outputlist))
            {
                try
                {
					DataTable dataTable = MPGlobals.GetDataForDatalist("exec ACSN_OpenItemsByAge");
					string jsonResult = JsonConvert.SerializeObject(dataTable);
					outputlist = JsonConvert.DeserializeObject<List<AcsnDashbord>>(jsonResult);

					_memoryCache.Set(cacheKey, outputlist, _memCacheOptions);
				}
                catch (Exception ex)
                {
                    ErrorHandler.LogException(ex);
                }  
            }
            return outputlist;
        }
        public static List<Master_General> getCachedMasterGeneral()
        {
            string cacheKey = CacheKeys.MasterGeneral.ToString();

            if (!_memoryCache.TryGetValue(cacheKey, out List<Master_General> outputlist))
            {
                try
                {
					using (DESI_STFE_PRODContext db = new())
					{
						outputlist = db.Master_Generals.AsNoTracking().ToList();
					}
					_memoryCache.Set(cacheKey, outputlist, _memCacheOptions);
				}
                catch (Exception ex)
                {
                    ErrorHandler.LogException(ex);
                }
            }
            return outputlist;
        }

    }
}
