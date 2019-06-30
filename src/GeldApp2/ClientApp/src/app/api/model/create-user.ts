export interface CreateUserCommand {
  name: string;
  password: string;
  createDefaultAccount: boolean;
}