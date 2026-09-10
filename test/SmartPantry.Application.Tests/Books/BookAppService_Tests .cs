using System;
using System.Linq;
using System.Threading.Tasks;
using Shouldly;
using SmartPantry.Authors;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Modularity;
using Volo.Abp.Validation;
using Xunit;

namespace SmartPantry.Books;

public abstract class BookAppService_Tests<TStartupModule> : SmartPantryApplicationTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{
    private readonly IBookAppService _bookAppService;
    private readonly IRepository<Author, Guid> _authorRepository;

    protected BookAppService_Tests()
    {
        _bookAppService = GetRequiredService<IBookAppService>();
        _authorRepository = GetRequiredService<IRepository<Author, Guid>>();
    }

    [Fact]
    public async Task Should_Get_List_Of_Books()
    {
        //Act
        var result = await _bookAppService.GetListAsync(
            new PagedAndSortedResultRequestDto()
        );

        //Assert
        result.TotalCount.ShouldBeGreaterThan(0);
        result.Items.ShouldContain(b => b.Name == "1984");
    }

    [Fact]
    public async Task Should_Create_A_Valid_Book()
    {
        //Arrange: Crear un autor válido para cumplir la Foreign Key en SQLite
        var author = await _authorRepository.InsertAsync(
            new Author
            {
                Name = "Test Author",
                BirthDate = new DateTime(1980, 1, 1),
                ShortBio = "Test Bio"
            },
            autoSave: true
        );

        //Act
        var result = await _bookAppService.CreateAsync(
            new CreateUpdateBookDto
            {
                AuthorId = author.Id,
                Name = "New test book 42",
                Price = 10,
                PublishDate = DateTime.Now,
                Type = BookType.ScienceFiction
            }
        );

        //Assert
        result.Id.ShouldNotBe(Guid.Empty);
        result.Name.ShouldBe("New test book 42");
    }
    
    [Fact]
    public async Task Should_Not_Create_A_Book_Without_Name()
    {
        var exception = await Assert.ThrowsAsync<AbpValidationException>(async () =>
        {
            await _bookAppService.CreateAsync(
                new CreateUpdateBookDto
                {
                    Name = "",
                    Price = 10,
                    PublishDate = DateTime.Now,
                    Type = BookType.ScienceFiction
                }
            );
        });

        exception.ValidationErrors
            .ShouldContain(err => err.MemberNames.Any(mem => mem == "Name"));
    }
}
