# Registration Checklist

- [ ] Use case class name ends with `UseCase`
- [ ] Class inherits one of:
  - `BaseInOutUseCase<TRequest, TResponse>`
  - `BaseInUseCase<TRequest>`
  - `BaseOutUseCase<TResponse>`
- [ ] Request type inherits `BaseRequest` where required
- [ ] Validator class exists and is public
- [ ] File is in `src/Application/{Context}/`
- [ ] No manual DI registration required (auto-scanned in `ApplicationDependencyInjection`)
