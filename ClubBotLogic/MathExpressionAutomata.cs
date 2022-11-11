namespace ClubBotLogic;

public class MathExpressionAutomata
{
    public abstract class State
    {
        internal abstract State Whitespace();
        internal abstract State Operator();
        internal abstract State Number();
        internal abstract State Other();
    }

    private class Init : State
    {
        internal override State Whitespace() => this;

        internal override State Operator() => new Terminate();

        internal override State Number() => new SeenNumber();

        internal override State Other() => new Terminate();
    }

    private class SeenNumber : State
    {
        internal override State Whitespace() => new SeenWhitespaceAfterNumber();

        internal override State Operator() => new SeenOperator();

        internal override State Number() => this;

        internal override State Other() => new Terminate();
    }

    private class SeenWhitespaceAfterNumber : State
    {
        internal override State Whitespace() => this;

        internal override State Operator() => new SeenOperator();

        internal override State Number() => new Terminate();

        internal override State Other() => new Terminate();
    }
    private class SeenWhitespaceAfterOperator : State
    {
        internal override State Whitespace() => this;

        internal override State Operator() => new Terminate();

        internal override State Number() => new SeenNumber();

        internal override State Other() => new Terminate();
    }

    private class SeenOperator : State
    {
        internal override State Whitespace() => new SeenWhitespaceAfterOperator();

        internal override State Operator() => new Terminate();

        internal override State Number() => new SeenNumber();

        internal override State Other() => new Terminate();
    }

    public class Terminate : State
    {
        internal override State Whitespace() => throw new NotImplementedException();

        internal override State Operator() => throw new NotImplementedException();

        internal override State Number() => throw new NotImplementedException();

        internal override State Other() => throw new NotImplementedException();
    }

    private string _input;
    private string _currentBuffer;
    private string _outBuffer;
    private bool _seenWhitespace;
    private State _currState = new Init();

    private IEnumerable<char> Operators = new[] { '+', '-', '*', '/' };
    private IEnumerable<char> Numbers = Enumerable.Range('0', 10).Select(i => (char)i);
    public MathExpressionAutomata(string input)
    {
        _input = input;
        _currentBuffer = "";
        _outBuffer = "";
    }

    public string ParseExpression()
    {
        foreach (var c in _input)
        {
            if (Operators.Contains(c))
            {
                _currState = _currState.Operator();
                _currentBuffer += c;
            }
            else if (Numbers.Contains(c))
            {
                _currState = _currState.Number();
                _currentBuffer += c;
                _outBuffer += _currentBuffer;
                _currentBuffer = "";
            }
            else if (c == ' ')
            {
                _currState = _currState.Whitespace();
                _currentBuffer += c;
            }
            else
            {
                _currState = _currState.Other();
            }

            if (_currState is Terminate) break;
        }

        return _outBuffer;
    }

    public State GetCurrentState()
    {
        return _currState;
    }
}