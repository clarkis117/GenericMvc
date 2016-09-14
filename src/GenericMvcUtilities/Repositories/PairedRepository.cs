using AutoMapper;
using GenericMvcUtilities.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace GenericMvcUtilities.Repositories
{
	/// <summary>
	/// This repository is meant for view models which are paired with files on disk
	/// Get with data should return the whole file
	///
	/// MimeTypes should be included in non get with data calls
	/// </summary>
	/// <seealso cref="GenericMvcUtilities.Repositories.IRepository{TViewModel}" />
	public abstract class PairedRepository<TKey, TViewModel, TEntity, TEntityRepo, TFileRepo> : IRepository<TViewModel>
		where TKey : IEquatable<TKey>
		where TViewModel : class, IModelFile<TKey>, new()
		where TEntity : class, IModelWithFilename<TKey>
		where TFileRepo : IFileRepository
		where TEntityRepo : IRepository<TEntity>
	{
		public readonly TEntityRepo _entityRepo;

		public readonly TFileRepo _fileRepo;

		protected readonly IMapper _mapper;

		private static readonly Type typeofT = typeof(TViewModel);

		public Type TypeOfEntity { get { return typeofT; } }

		private static readonly ParameterExpression expressionOfT = Expression.Parameter(typeofT);

		public ParameterExpression EntityExpression { get { return expressionOfT; } }

		public PairedRepository(TFileRepo filerepo, TEntityRepo dbrepo)
		{
			if (filerepo == null)
			{
				throw new ArgumentNullException(nameof(filerepo));
			}

			if (filerepo == null)
			{
				throw new ArgumentNullException(nameof(filerepo));
			}

			_fileRepo = filerepo;

			_entityRepo = dbrepo;

			var config = MapperConfig;

			try
			{
				config.AssertConfigurationIsValid();
			}
			catch (Exception e)
			{
				throw new Exception("Mapper Config is invalid", e);
			}

			_mapper = config.CreateMapper();
		}

		protected static readonly MapperConfiguration mapperConfig = new MapperConfiguration(cfg =>
		{
			cfg.CreateMap<TViewModel, TEntity>();

			cfg.CreateMap<TEntity, TViewModel>();
		});

		protected virtual MapperConfiguration MapperConfig { get { return mapperConfig; } }

		protected Expression<Func<TEntity, bool>> translateExpression(Expression<Func<TViewModel, bool>> predicate)
		{
			return _mapper.Map<Expression<Func<TEntity, bool>>>(predicate);
		}

		/// <summary>
		/// Maps the properties, for operations where the file needs dealt with first
		/// instead of the sql database
		/// </summary>
		/// <param name="viewModelWithFileData">The view model with file data.</param>
		/// <param name="updatedEntity">The updated entity.</param>
		/// <returns></returns>
		protected abstract TViewModel addEntitySection(TViewModel viewModelWithFileData, TEntity updatedEntity);

		protected abstract TViewModel ConvertProperties(TEntity entity);

		protected abstract Task<TViewModel> Convert(TEntity entity, bool WithData = false);

		protected abstract TEntity Convert(TViewModel viewModel);

		private IObservable<TViewModel> GetViewModelsObservable()
		{
			return Observable.Create<TViewModel>(async obs =>
			{
				using (var enumerator = _entityRepo.GetEnumerator())
				{
					while (enumerator.MoveNext())
					{
						var entity = enumerator.Current;

						var viewModel = await Convert(entity);

						obs.OnNext(viewModel);
					}
				}

				obs.OnCompleted();
			});
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetViewModelsObservable().GetEnumerator();
		}

		public IEnumerator<TViewModel> GetEnumerator()
		{
			return GetViewModelsObservable().GetEnumerator();
		}

		public Task<IEnumerable<TViewModel>> GetAll()
		{
			return Task.FromResult(GetViewModelsObservable().ToEnumerable());
		}

		public Task<bool> Any(Expression<Func<TViewModel, bool>> predicate)
		{
			if (predicate != null)
			{
				try
				{
					var expression = translateExpression(predicate);

					return _entityRepo.Any(expression);
				}
				catch (Exception e)
				{
					throw new Exception($"Operation on {typeof(TViewModel).Name} Failed", e);
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(predicate));
			}
		}

		public async Task<TViewModel> Get(Expression<Func<TViewModel, bool>> predicate)
		{
			if (predicate != null)
			{
				try
				{
					var expression = translateExpression(predicate);

					var entity = await _entityRepo.Get(expression);

					return await Convert(entity);
				}
				catch (Exception e)
				{
					throw new Exception($"Operation on {typeof(TViewModel).Name} Failed", e);
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(predicate));
			}
		}

		public async Task<TViewModel> Get(Expression<Func<TViewModel, bool>> predicate, bool WithNestedData = false)
		{
			if (predicate != null)
			{
				try
				{
					if (WithNestedData)
					{
						var expression = translateExpression(predicate);

						var entity = await _entityRepo.Get(expression);

						return await Convert(entity, WithData: true);
					}
					else
					{
						return await this.Get(predicate);
					}
				}
				catch (Exception e)
				{
					throw new Exception($"Operation on {typeof(TViewModel).Name} Failed", e);
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(predicate));
			}
		}

		public async Task<IList<TViewModel>> GetMany(Expression<Func<TViewModel, bool>> predicate)
		{
			if (predicate != null)
			{
				try
				{
					var expression = translateExpression(predicate);

					var entities = await _entityRepo.GetMany(expression);

					var list = new TViewModel[entities.Count()];

					for (int i = 0; i < list.Count(); i++)
					{
						list[i] = ConvertProperties(entities[i]);
					}

					return new List<TViewModel>(list);
				}
				catch (Exception e)
				{
					throw new Exception($"Operation on {typeof(TViewModel).Name} Failed", e);
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(predicate));
			}
		}

		public async Task<IList<TViewModel>> GetMany(Expression<Func<TViewModel, bool>> predicate, bool WithNestedData = false)
		{
			if (predicate != null)
			{
				try
				{
					if (WithNestedData)
					{
						var expression = translateExpression(predicate);

						var entities = await _entityRepo.GetMany(expression);

						var list = new TViewModel[entities.Count()];

						for (int i = 0; i < list.Count(); i++)
						{
							list[i] = await Convert(entities[i], WithData: true);
						}

						return new List<TViewModel>(list);
					}
					else
					{
						return await GetMany(predicate);
					}
				}
				catch (Exception e)
				{
					throw new Exception($"Operation on {typeof(TViewModel).Name} Failed", e);
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(predicate));
			}
		}

		//this needs to retrun the proper id with the view model
		public virtual async Task<TViewModel> Create(TViewModel viewModel)
		{
			if (viewModel != null)
			{
				try
				{
					var dbEntity = Convert(viewModel);

					var createdEntity = await _entityRepo.Create(dbEntity);

					await _fileRepo.Create(viewModel);

					if (createdEntity != null && viewModel != null)
					{
						viewModel = addEntitySection(viewModel, createdEntity);

						return viewModel;
					}
					else
					{
						throw new Exception("something failed in creating on data storage");
					}
				}
				catch (Exception e)
				{
					throw new Exception($"Operation on {typeof(TViewModel).Name} Failed", e);
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(viewModel));
			}
		}

		public async Task<IEnumerable<TViewModel>> CreateRange(IEnumerable<TViewModel> viewModels)
		{
			if (viewModels != null)
			{
				try
				{
					var list = new List<TViewModel>();

					foreach (var entity in viewModels)
					{
						list.Add(await Create(entity));
					}

					return list;
				}
				catch (Exception e)
				{
					throw new Exception($"Operation on {typeof(TViewModel).Name} Failed", e);
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(viewModels));
			}
		}

		/// <summary>
		/// Deletes the specified view model.
		/// Entities with relationships to other tables should override this to deal with those relationships
		/// i.e. if the entity is still related to other entities do not delete it just remove the association to the 
		/// current context in which it is being deleted
		/// </summary>
		/// <param name="viewModel">The view model.</param>
		/// <returns></returns>
		/// <exception cref="System.Exception"></exception>
		/// <exception cref="System.ArgumentNullException"></exception>
		public virtual async Task<bool> Delete(TViewModel viewModel)
		{
			if (viewModel != null)
			{
				try
				{
					var entity = Convert(viewModel);

					var dbResult = await _entityRepo.Delete(entity);

					//entity Should be deleted first
					var fileResult = await _fileRepo.Delete(viewModel);

					if (dbResult && fileResult)
					{
						return true;
					}
					else
					{
						return false;
					}
				}
				catch (Exception e)
				{
					throw new Exception($"Operation on {typeof(TViewModel).Name} Failed", e);
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(viewModel));
			}
		}

		public abstract Task<bool> CascadeDelete(TViewModel viewModel);

		public async Task<bool> DeleteRange(IEnumerable<TViewModel> viewModels)
		{
			if (viewModels != null)
			{
				try
				{
					var list = new List<bool>();

					foreach (var viewModel in viewModels)
					{
						list.Add(await Delete(viewModel));
					}

					if (list.Any(x => x == false))
					{
						return false;
					}
					else
					{
						return true;
					}
				}
				catch (Exception e)
				{
					throw new Exception($"Operation on {typeof(TViewModel).Name} Failed", e);
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(viewModels));
			}
		}

		//photo repo should just have to override this to generate md5 based name
		//todo update the photo repo to work with this
		//if the actual file changed delete it then create a new one
		public virtual async Task<TViewModel> Update(TViewModel viewModel)
		{
			if (viewModel != null)
			{
				try
				{
					//if underlying file changed delete it and create a new one
					//if it didn't use the regular file repo update method
					//check to see if the underlying file changed on us
					var id = viewModel.Id; //since auto mapper doesn't like non constants 

					var oldEntity = await _entityRepo.Get(x=> x.Id.Equals(id));

					//check to see if file changed
					if (viewModel.Name == oldEntity.Filename)
					{
						//use regular file repo update
						//then try to update the file
						await _fileRepo.Update(viewModel);
					}
					else // delete file and make a new one
					{
						var deleteResult = await _fileRepo.Delete(await Convert(oldEntity));

						if (deleteResult)
						{
							await _fileRepo.Create(viewModel);
						}
						else
						{
							throw new System.IO.IOException("Could not delete old file");
						}
					}

					//Now update sql data model if the file op was successfull
					var updatedEntity = await _entityRepo.Update(Convert(viewModel));

					var updatedViewModel = addEntitySection(viewModel, updatedEntity);

					if (updatedViewModel != null)
					{
						return updatedViewModel;
					}
					else
					{
						throw new Exception("error occured updating file or db record");
					}
				}
				catch (Exception e)
				{
					throw new Exception($"Operation on {typeof(TViewModel).Name} Failed", e);
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(viewModel));
			}
		}

		public async Task<IEnumerable<TViewModel>> UpdateRange(IEnumerable<TViewModel> viewModels)
		{
			if (viewModels != null)
			{
				try
				{
					var list = new List<TViewModel>();

					foreach (var entity in viewModels)
					{
						list.Add(await Update(entity));
					}

					return list;
				}
				catch (Exception e)
				{
					throw new Exception($"Operation on {typeof(TViewModel).Name} Failed", e);
				}
			}
			else
			{
				throw new ArgumentNullException(nameof(viewModels));
			}
		}

		public Task<long> Count()
		{
			return _entityRepo.Count();
		}
	}
}