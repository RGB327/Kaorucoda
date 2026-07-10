// Cognito User Pool "Pre sign-up" 트리거.
// 회원가입 직후 자동으로 계정을 확인(Confirmed) 상태로 만들어서
// 이메일 인증 절차 없이 바로 로그인(InitiateAuth)이 가능하게 한다.
export const handler = async (event) => {
  event.response.autoConfirmUser = true;
  return event;
};
