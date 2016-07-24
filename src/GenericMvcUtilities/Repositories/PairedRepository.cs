using AutoMapper;
using GenericMvcUtilities.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reactive.Linq;
using System.Threading.Tasks;

namespace GenericMvcUtilities.Repositories
{
	/// <summary>
	/// Get with data should return the whole file
	///
	/// MimeTypes should be included in non get with data calls
	/// </summary>
	/// <seealso cref="GenericMvcUtilities.Repositories.IRepository{TViewModel}" />
	public abstract class PairedRepository<TKey, TViewModel, TEntity, TEntityRepo, TFileRepo> : IRepository<TViewModel>
		where TKey : IEquatable<TKey>
		where TViewModel : class, IModelWithFile<TKey>
		where TEntity : class, IModel<TKey>
		where TFileRepo : FileRepository
		where TEntityRepo : IRepository<TEntity>
	{
		protected TEntityRepo _entityRepo;

		protected TFileRepo _fileRepo;

		protected IMapper _mapper;

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

			var config = getMapperConfig();

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

		protected abstract MapperConfiguration getMapperConfig();

		/*
		{
			return new MapperConfiguration(cfg =>
			cfg.CreateMap<TViewModel, TEntity>()
				.ForMember(x => x.Filename, conf => conf.MapFrom(ol => ol.File.Name)));
		}
		*/

		protected Expression<Func<TEntity, bool>> translateExpression(Expression<Func<TViewModel, bool>> predicate)
		{
			return _mapper.Map<Expression<Func<TEntity, bool>>>(predicate);
		}

		protected abstract TViewModel ConvertProperties(TEntity entity);

		/*
		{
			var viewModel = new Document();

			viewModel.Id = doc.Id;

			viewModel.Title = doc.Title;

			viewModel.Description = doc.Description;

			return viewModel;
		}
		*/

		protected abstract Task<TViewModel> Convert(TEntity entity, bool WithData = false);

		/*
	{
		var viewModel = Convert(doc);

		viewModel.File = await _fileRepo.Get(x => x.Name == doc.Filename);

		return viewModel;
	}
	*/

		/// <summary>
		/// !!!File will still need save seperately
		/// Converts the specified document.
		/// </summary>
		/// <param name="doc">The document.</param>
		/// <returns></returns>
		protected abstract TEntity Convert(TViewModel viewModel);

		/*
	{
		var model = new Models.Content.Documents();

		model.Id = doc.Id;

		model.Title = doc.Title;

		model.Description = doc.Description;

		model.Filename = doc.File.Name;

		return model;
	}
	*/

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

		public virtual async Task<TViewModel> Create(TViewModel entity)
		{
			if (entity != null)
			{
				try
				{
					var dbEntity = Convert(entity);

					var dbResult = await _entityRepo.Create(dbEntity);

					var fileResult = await _fileRepo.Create(entity.File);

					if (dbResult != null && fileResult != null)
					{
						var viewModel = ConvertProperties(dbResult);

						viewModel.File = fileResult;

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
				throw new ArgumentNullException(nameof(entity));
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

		public virtual async Task<bool> Delete(TViewModel viewModel)
		{
			if (viewModel != null)
			{
				try
				{
					var entity = Convert(viewModel);

					var dbResult = await _entityRepo.Delete(entity);

					var fileResult = await _fileRepo.Delete(viewModel.File);

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

		public virtual async Task<TViewModel> Update(TViewModel viewModel)
		{
			if (viewModel != null)
			{
				try
				{
					var entity = Convert(viewModel);

					var dbResult = await _entityRepo.Update(entity);

					var fileResult = await _fileRepo.Update(viewModel.File);

					if (dbResult != null && fileResult != null)
					{
						var newViewModel = ConvertProperties(dbResult);

						newViewModel.File = fileResult;

						return newViewModel;
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
	}
}