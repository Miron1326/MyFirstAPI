namespace MyAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddSingleton<ICounter, Counter>();//DI Singletone счетчик (интерфейс, класс интерфейса)

            var app = builder.Build();




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
