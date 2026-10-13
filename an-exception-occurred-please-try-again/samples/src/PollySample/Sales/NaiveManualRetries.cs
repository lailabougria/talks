namespace Sales;

public static class NaiveManualRetries
{
    public static void WithRetries(this Action action, int nrOfAttempts)
    {
        for (var attempt = 0; attempt < nrOfAttempts; attempt++)
        {
            try
            {
                // Invoke the action
                action();
                return;
            }
            catch (Exception)
            {
                // swallow exception
            }
        }
        
        // This is the last retry, outside the try catch to ensure any failures will bubble up to the caller
        action();
    }

    public static void WithDelayedRetries(this Action action, int nrOfAttempts)
    {
        for (var attempt = 0; attempt < nrOfAttempts; attempt++)
        {
            try
            {
                // Invoke the action
                action();
                return;
            }
            catch (Exception)
            {
                // swallow exception and wait
                Thread.Sleep(TimeSpan.FromMilliseconds(500 * (attempt + 1)));
            }
        }
        
        // This is the last retry, outside the try catch to ensure any failures will bubble up to the caller
        action();
    }
    
    public static void WithDelayedRetriesAndJitter(this Action action, int nrOfAttempts)
    {
        for (var attempt = 0; attempt < nrOfAttempts; attempt++)
        {
            try
            {
                // Invoke the action
                action();
                return;
            }
            catch (Exception)
            {
                // swallow exception and wait
                Thread.Sleep(TimeSpan.FromMilliseconds(Random.Shared.Next(200, 800) * (attempt + 1)));
            }
        }
        
        // This is the last retry, outside the try catch to ensure any
        // failures will bubble up
        action();
    }

    public static void WithCombinedRetries(this Action action, int nrOfImmediateAttempts, int nrOfDelayedAttempts)
    {
        for (var attempt = 0; attempt < nrOfDelayedAttempts; attempt++)
        {
            try
            {
                WithRetries(action, nrOfImmediateAttempts);
                return;
            }
            catch (Exception)
            {
                // swallow exception and wait
                Thread.Sleep(TimeSpan.FromMilliseconds(Random.Shared.Next(200, 800) * (attempt + 1)));
            }
        }
        
        // Last set of immediate retries, outside the try catch
        WithRetries(action, nrOfImmediateAttempts);
    }
}