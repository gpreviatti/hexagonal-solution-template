# Status Code Rules

- `GET /{id}`: `200 OK` on success, `404 NotFound` on failure
- `POST /`: `201 Created` when response contains `Data`, otherwise `400 BadRequest`
- `POST /paginated`: `200 OK` on success, `400 BadRequest` on failure
- `PUT /{id}`: `200 OK` on success, `400 BadRequest` on failure
- `DELETE /{id}`: `200 OK` on success, `400 BadRequest` on failure
