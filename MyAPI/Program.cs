using Microsoft.EntityFrameworkCore;
using MyAPI.Models;

namespace MyAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddDbContext<AppDbContext>(options => options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));//ложим AppDbContext в builder.Servises, из appsettings.json

            builder.Services.AddSingleton<ICounter, Counter>();//DI Singletone счетчик (интерфейс, класс интерфейса)

            builder.Services.AddSwaggerGen();

            var app = builder.Build();
            app.UseSwagger();
            app.UseSwaggerUI();



            app.Use(async (context, next) =>
            {
                Console.WriteLine($"Вошёл {context.Request.Method} {context.Request.Path}");
                await next(); //по факту команда проходи дальше
                Console.WriteLine($"Вышел {context.Response.StatusCode}");
            }); //заход

            app.Use(async (context, next) =>
            {
                var password = context.Request.Query["password"].ToString();
                if(context.Request.Path == "/secret" &&  password != "1234")
                {
                    context.Response.StatusCode = 401;//не авторизован
                    await context.Response.WriteAsync("NonAUTH");
                    return;
                }
                await next();
                
            }); //проверка


     //получить предметы

            app.MapGet("/inventory", async (AppDbContext db) =>
            {
                //ToListAsync - переводит SQL запрос в список C#
                var items = await db.Items.ToListAsync();
                return Results.Ok(items);
            });



     //добавить предмет

            app.MapPost("/inventory", async (AppDbContext db, Item newItem) =>
            {
                db.Items.Add(newItem);
                await db.SaveChangesAsync(); //Сохраняем изменения в таблице и базу данных
                return Results.Created($"/inventory/{newItem}", newItem);
            });


     //удалить предмет

            app.MapDelete("/inventory/{id}", async (AppDbContext db, int id) =>
            {
                var item = await db.Items.FindAsync(id);
                if(item is null)
                {
                    return Results.NotFound("Предмет не найден");
                }
                db.Items.Remove(item);
                
                await db.SaveChangesAsync();
                return Results.NoContent();
            });

     //полностью заменить предмет

            app.MapPut("/inventory/{id}", async (AppDbContext db, int id, Item updatedItem) =>
            {
                var existingItem = await db.Items.FindAsync(id); //поиск по id
                if(existingItem is null)
                {
                    return Results.NotFound($"Предмет с id {id} не найден");
                }

                existingItem.Name = updatedItem.Name;
                existingItem.Quantity = updatedItem.Quantity;

                await db.SaveChangesAsync();
                return Results.Ok(existingItem);
            });

     //обычные серверы

            app.MapGet("/", (ICounter counter) =>
            {
                counter.Increment();
                return $"Счётчик запросов: {counter.Value}";
            }); //запрашивает DI прям в лямбде

            app.MapGet("/reset", (ICounter counter) =>
            {
                counter.Reset();
                return $"Счётчик сброшен";
            });



            app.MapGet("/secret", () => "Данные под паролём");

            app.Run();//Kestel сервер
        }
    }

    public interface ICounter //что умеет счетчик
    {
        int Value { get; }
        void Increment();
        void Reset();
    }

    public class Counter : ICounter
    {
        public int Value {  get; private set; }

        public void Increment()
        {
            Value++;
            Console.WriteLine($"[Counter] увеличил до {Value}");
        }

        public void Reset()
        {
            Value = 0;
            Console.WriteLine($"[Counter] сброшен");
        }
    }

}
