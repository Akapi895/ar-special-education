/**
 * Mock Child Data
 * Child profile for demo
 */

export interface ChildProfile {
  id: string;
  name: string;
  dateOfBirth: string;
  age: number;
  dyscalculiaLevel: 'nhẹ' | 'trung bình' | 'nặng';
  dyscalculiaTypes: string[];
  createdAt: string;
  notes?: string;
}

export const mockChild: ChildProfile = {
  id: 'child-001',
  name: 'Minh',
  dateOfBirth: '2021-03-15',
  age: 5,
  dyscalculiaLevel: 'trung bình',
  dyscalculiaTypes: [
    'Khó khăn trong việc hiểu ý nghĩa của số lượng',
    'Nhầm lẫn các số có hình dạng tương tự (6 và 9)',
    'Cần thời gian lâu hơn để xử lý phép tính',
    'Tính toán chậm với số lớn hơn 5',
  ],
  createdAt: '2026-05-01',
  notes: 'Minh là bé rất ngoan và chăm chỉ. Bé thích học khi được khen ngợi. Cần kiên nhẫn, không ép bé quá sức.',
};

export default mockChild;
