using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using GenericMvcUtilities.Test.Lib;
using GenericMvcUtilities.Test.Lib.Models;
using GenericMvcUtilities.Test.Lib.Repository;
using Xunit;
using GenericMvcUtilities.Test.Lib.Fixtures;
using GenericMvcUtilities.Test.Lib.Contexts;
using GenFu;

namespace GenericMvcUtilities.Tests.Repositories
{
	public class EntityFramework : IRepositoryTests<Blog, BlogRepo, BlogDbContext, DataBaseFixture<BlogDbContext>>
	{
		public EntityFramework() : base()
		{
			
		}

		protected override Blog Mutator(Blog entity)
		{
			throw new NotImplementedException();
		}

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

		[Fact]
		public override Task GetManyWithData()
		{
			throw new NotImplementedException();
		}

		public override Task GetWithData()
		{
			throw new NotImplementedException();
		}

		protected override Expression<Func<Blog, bool>> GetManyQuery()
		{
			throw new NotImplementedException();
		}

		protected override Expression<Func<Blog, bool>> GetQuery()
		{
			throw new NotImplementedException();
		}
	}
}
