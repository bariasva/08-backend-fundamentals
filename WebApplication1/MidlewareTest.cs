
public class MidlewareTest
{
    private readonly RequestDelegate _next;
    public MidlewareTest(RequestDelegate next)
    {
        _next = next;
    }
    public async Task Invoke(HttpContext context)
    {
        Console.WriteLine("----------> This is a TESTTSET a si sihT <----------");
        await _next(context);
    }
};