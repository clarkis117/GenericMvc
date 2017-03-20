using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GenericMvc.Test.Lib;
using GenericMvc.Test.Lib.Models;
using GenericMvc.Test.Lib.Repository;
using Xunit;
using GenericMvc.Test.Lib.Fixtures;
using GenericMvc.Test.Lib.Contexts;
using GenFu;

namespace GenericMvc.Tests.Repositories
{
	public class EntityFramework : EntityRepository<Blog, int, BlogRepo, BlogDbContext, DataBaseFixture<BlogDbContext>>
	{
		public EntityFramework() : base()
		{

		}

		protected override Blog Mutator(Blog entity) => entity;

		protected override Blog CreateObjectGraph(int n)
		{
			var blog = A.New<Blog>();

			blog.Owner = A.New<User>();

			blog.Posts = A.ListOf<BlogPost>(n);

			foreach (var post in blog.Posts)
			{
				//post.Blog = blog;

				post.Comments = A.ListOf<Comment>(n);

				foreach (var comment in post.Comments)
				{
					//comment.Post = post;

					comment.User = A.New<User>();

					comment.Replies = A.ListOf<Comment>(n);

					foreach (var reply in comment.Replies)
					{
						reply.User = A.New<User>();

						//reply.Post = post;
					}
				}
			}

			return blog;
		}

		protected override IEnumerable<Blog> CreateListofGraphs(int numberOfObjects, int numberOfSubObjects)
		{
			var list = new List<Blog>();

			for (int i = 0; i < numberOfObjects; i++)
			{
				var item = CreateObjectGraph(numberOfSubObjects);

				list.Add(item);
			}

			return SantitizeData(list);
		}

		protected override IEnumerable<Blog> SantitizeData(IEnumerable<Blog> collection)
		{
			foreach (var item in collection)
			{
				item.Id = 0;

				if (item.Owner != null)
				{
					item.Owner.Id = Guid.Empty;
				}

				if(item.Posts != null)
				{
					foreach (var post in item.Posts)
					{
						post.Id = 0;

						foreach (var comment in post.Comments)
						{
							comment.Id = 0;

							comment.User.Id = Guid.Empty;

							foreach (var reply in comment.Replies)
							{
								reply.Id = 0;

								reply.User.Id = Guid.Empty;
							}
						}
					}
				}
			}

			return collection;
		}


		public override Task GetManyWithData()
		{
			return Task.FromResult<object>(null);
		}

		public override Task GetWithData()
		{
			return Task.FromResult<object>(null);
		}


		protected override Expression<Func<Blog, bool>> GetManyQuery()
		{
			return null;
		}


		protected override Expression<Func<Blog, bool>> GetQuery()
		{
			return null;
		}

		public override Task GetMany()
		{
			throw new NotImplementedException();
		}
	}
}
