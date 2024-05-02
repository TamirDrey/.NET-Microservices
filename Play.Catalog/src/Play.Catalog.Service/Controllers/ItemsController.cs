using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Play.Catalog.Service.Dtos;
using Play.Catalog.Service.Entities;
using Play.Common;
using static Play.Catalog.Contracts.Contracts;


namespace Play.Catalog.Service.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly IRepository<Item> itemsRepository;
        private readonly IPublishEndpoint publishEndpoint;


        public ItemsController(IRepository<Item> itemsRepository, IPublishEndpoint publishEndpoint)
        {
            this.itemsRepository = itemsRepository;
            this.publishEndpoint = publishEndpoint;
        }

        // GET http://localhost:[port]/api/items
        [HttpGet]
        public async Task<IEnumerable<ItemDto>> Get()
        {
            var items = (await itemsRepository.GetAllAsync()).Select(items => items.AsDto());
            return items;
        }

        // GET http://localhost:[port]/api/items/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<ItemDto>> GetById(Guid id)
        {
            var item = await itemsRepository.GetAsynq(id);

            if (item == null) return NotFound();

            return item.AsDto();
        }

        // POST http://localhost:[port]/api/items   
        [HttpPost]
        public async Task<ActionResult<ItemDto>> Post(CreateItemDto createItemDto)
        {
            var item = new Item
            {
                Name = createItemDto.Name,
                Description = createItemDto.Description,
                Price = createItemDto.Price,
                CreatedDate = DateTimeOffset.UtcNow
            };

            await itemsRepository.CreateAsync(item);
            await publishEndpoint.Publish(new CatalogItemCreated(item.Id, item.Name, item.Description));

            return CreatedAtAction(nameof(GetById), new { id = item.Id }, item.AsDto());
        }

        // PUT http://localhost:[port]/api/items/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, UpdateItemDto updateItemDto)
        {
            var existingItem = await itemsRepository.GetAsynq(id);

            if (existingItem == null) return NotFound();

            existingItem.Name = updateItemDto.Name;
            existingItem.Description = updateItemDto.Description;
            existingItem.Price = updateItemDto.Price;

            await itemsRepository.UpdateAsync(existingItem);
            await publishEndpoint.Publish(new CatalogItemUpdated(existingItem.Id, existingItem.Name, existingItem.Description));

            return NoContent();
        }

        // DELETE http://localhost:[port]/api/items/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var item = await itemsRepository.GetAsynq(id);

            if (item == null) return NotFound();

            await itemsRepository.RemoveAsync(item.Id);

            return NoContent();
        }
    }
}