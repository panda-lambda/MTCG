using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Utilities
{

    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message) { }
        public UnauthorizedException() : base() { }
    }


    public class InternalServerErrorException : Exception
    {
        public InternalServerErrorException(string message) : base(message) { }
    }

    public class InvalidCardCountInDeck : Exception
    {
        public InvalidCardCountInDeck(string message) : base(message) { }
    }
    public class UserNotCardOwnerException : Exception
    {
        public UserNotCardOwnerException(string message) : base(message) { }
            }

    public class ForbiddenException : Exception
    {
        public ForbiddenException(string message) : base(message) { }
    }
    public class UserCurrentlyFightingException : Exception
    {
        public UserCurrentlyFightingException() : base() { }
    }

    public class BattleResultIsNullException : Exception
    {
        public BattleResultIsNullException() : base() { }
    }

    public class InvalidCardForDealException : Exception
    {
        public InvalidCardForDealException() : base() { }
    }

    public class NotFoundException : Exception
    {
        public NotFoundException(string message) : base(message) { }
    }
    public class ConflictException : Exception
    {
        public ConflictException(string message) : base(message) { }
    }

    public class NoContentException : Exception
    {
        public NoContentException(string message) : base(message) { }
    }

    public class BadRequestException : Exception
    {
        public BadRequestException(string message) : base(message) { }
    }

}
