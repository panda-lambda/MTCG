﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MTCG.Utilities
{
    public class PackageNotFoundException : Exception
    {
        public PackageNotFoundException(string message) : base(message) { }
    }

    public class UnauthorizedException : Exception
    {
        public UnauthorizedException(string message) : base(message) { }
        public UnauthorizedException() : base() { }
    }
    public class InsufficientCoinsException : Exception
    {
        public InsufficientCoinsException(string message) : base(message) { }
    }
    public class NoAvailableCardsException : Exception
    {
        public NoAvailableCardsException(string message) : base(message) { }
    }
    public class InternalServerErrorException : Exception
    {
        public InternalServerErrorException(string message) : base(message) { }
    }
    public class UserHasNoCardsException : Exception
    {
        public UserHasNoCardsException(string message) : base(message) { }
    }
    public class InvalidCardCountInDeck : Exception
    {
        public InvalidCardCountInDeck(string message) : base(message) { }
    }
    public class UserNotCardOwnerException : Exception
    {
        public UserNotCardOwnerException(string message) : base(message) { }
    }
    public class UserNotAdminException : Exception
    {
        public UserNotAdminException(string message) : base(message) { }
    }
    public class CardExistsAlreadyException : Exception
    {
        public CardExistsAlreadyException(string message) : base(message) { }
    }
    public class UserNotFoundException : Exception
    {
        public UserNotFoundException(string message) : base(message) { }
    }

    public class UserHasNoValidDeckException : Exception
    {
        public UserHasNoValidDeckException(string message) : base(message) { }
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
