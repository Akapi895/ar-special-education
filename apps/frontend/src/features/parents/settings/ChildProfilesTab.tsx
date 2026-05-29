/**
 * Child Profiles Tab
 * Simplified for MathMate Support - single child
 */

import { useState } from 'react';
import { Edit2 } from 'lucide-react';
import Button from '../../../components/ui/Button';
import { mockChild } from '../../../api/mockData';

interface ChildProfilesTabProps {
  onSave?: () => void;
}

const ChildProfilesTab = ({ onSave }: ChildProfilesTabProps) => {
  const [child, setChild] = useState(mockChild);
  const [isEditing, setIsEditing] = useState(false);

  const dyscalculiaLevels = [
    { value: 'nhẹ', label: 'Nhẹ' },
    { value: 'trung bình', label: 'Trung bình' },
    { value: 'nặng', label: 'Nặng' },
  ];

  const dyscalculiaTypes = [
    'Không hiểu ý nghĩa của số lượng',
    'Nhầm lẫn các số có hình dạng tương tự (6 và 9)',
    'Tính toán chậm với số lớn hơn 5',
    'Khó nhớ các phép tính cơ bản',
    'Sợ học toán (math anxiety)',
    'Không hiểu đề bài',
  ];

  const handleSave = () => {
    setIsEditing(false);
    // In real app, save to localStorage
    onSave?.();
  };

  return (
    <div className="space-y-6">
      {/* Child Profile Card */}
      <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6">
        <div className="flex items-start justify-between mb-6">
          <div className="flex items-center gap-4">
            <div className="w-16 h-16 bg-gradient-to-br from-blue-100 to-purple-100 rounded-full flex items-center justify-center text-3xl">
              👶
            </div>
            <div>
              <h2 className="text-xl font-bold text-gray-900">{child.name}</h2>
              <p className="text-gray-500">
                {child.age} tuổi • Sinh nhật: {new Date(child.dateOfBirth).toLocaleDateString('vi-VN')}
              </p>
            </div>
          </div>
          <Button variant="secondary" size="sm" onClick={() => setIsEditing(!isEditing)}>
            <Edit2 className="w-4 h-4 mr-2" />
            {isEditing ? 'Hủy' : 'Chỉnh sửa'}
          </Button>
        </div>

        {isEditing ? (
          <div className="space-y-4">
            {/* Name */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Tên</label>
              <input
                type="text"
                value={child.name}
                onChange={(e) => setChild({ ...child, name: e.target.value })}
                className="w-full px-4 py-2 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
              />
            </div>

            {/* Date of Birth */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Ngày sinh</label>
              <input
                type="date"
                value={child.dateOfBirth}
                onChange={(e) => setChild({ ...child, dateOfBirth: e.target.value })}
                className="w-full px-4 py-2 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
              />
            </div>

            {/* Dyscalculia Level */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Mức độ Dyscalculia
              </label>
              <div className="flex gap-3">
                {dyscalculiaLevels.map((level) => (
                  <label key={level.value} className="flex-1 cursor-pointer">
                    <input
                      type="radio"
                      name="level"
                      value={level.value}
                      checked={child.dyscalculiaLevel === level.value}
                      onChange={(e) => setChild({ ...child, dyscalculiaLevel: e.target.value as typeof child.dyscalculiaLevel })}
                      className="peer hidden"
                    />
                    <div className="px-4 py-3 border-2 border-gray-200 rounded-xl text-center peer-checked:border-primary-500 peer-checked:bg-primary-50 transition-colors">
                      {level.label}
                    </div>
                  </label>
                ))}
              </div>
            </div>

            {/* Dyscalculia Types */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-2">
                Khó khăn cụ thể
              </label>
              <div className="space-y-2">
                {dyscalculiaTypes.map((type) => (
                  <label key={type} className="flex items-center gap-3 cursor-pointer">
                    <input
                      type="checkbox"
                      checked={child.dyscalculiaTypes.includes(type)}
                      onChange={(e) => {
                        if (e.target.checked) {
                          setChild({
                            ...child,
                            dyscalculiaTypes: [...child.dyscalculiaTypes, type],
                          });
                        } else {
                          setChild({
                            ...child,
                            dyscalculiaTypes: child.dyscalculiaTypes.filter((t) => t !== type),
                          });
                        }
                      }}
                      className="w-5 h-5 rounded border-gray-300 text-primary-600 focus:ring-primary-500"
                    />
                    <span className="text-gray-700">{type}</span>
                  </label>
                ))}
              </div>
            </div>

            {/* Notes */}
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">Ghi chú</label>
              <textarea
                value={child.notes || ''}
                onChange={(e) => setChild({ ...child, notes: e.target.value })}
                rows={3}
                className="w-full px-4 py-2 border border-gray-200 rounded-lg focus:outline-none focus:ring-2 focus:ring-primary-500"
                placeholder="Ghi chú về con..."
              />
            </div>

            <div className="flex justify-end gap-3">
              <Button variant="secondary" onClick={() => setIsEditing(false)}>
                Hủy
              </Button>
              <Button onClick={handleSave}>
                Lưu thay đổi
              </Button>
            </div>
          </div>
        ) : (
          <div className="space-y-4">
            {/* Dyscalculia Level */}
            <div className="p-4 bg-blue-50 rounded-xl">
              <p className="text-sm font-medium text-gray-700 mb-1">Mức độ Dyscalculia</p>
              <p className="text-lg font-semibold text-blue-700 capitalize">
                {child.dyscalculiaLevel}
              </p>
            </div>

            {/* Dyscalculia Types */}
            <div>
              <p className="text-sm font-medium text-gray-700 mb-2">Khó khăn cụ thể</p>
              <div className="flex flex-wrap gap-2">
                {child.dyscalculiaTypes.map((type) => (
                  <span
                    key={type}
                    className="px-3 py-1 bg-gray-100 text-gray-700 rounded-full text-sm"
                  >
                    {type}
                  </span>
                ))}
              </div>
            </div>

            {/* Notes */}
            {child.notes && (
              <div>
                <p className="text-sm font-medium text-gray-700 mb-1">Ghi chú</p>
                <p className="text-gray-600">{child.notes}</p>
              </div>
            )}
          </div>
        )}
      </div>

      {/* Reset Data Section */}
      <div className="bg-white rounded-2xl shadow-sm border border-red-200 p-6">
        <h3 className="text-lg font-bold text-gray-900 mb-2">Đặt lại dữ liệu</h3>
        <p className="text-sm text-gray-600 mb-4">
          ⚠️ Cảnh báo: Các hành động dưới đây không thể hoàn tác!
        </p>
        <div className="space-y-3">
          <Button variant="danger" className="w-full justify-center">
            Đặt lại tiến độ học tập
          </Button>
          <Button variant="danger" className="w-full justify-center">
            Xóa tất cả nhật ký
          </Button>
        </div>
      </div>
    </div>
  );
};

export default ChildProfilesTab;
